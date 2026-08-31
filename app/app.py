import os
import struct

import pyodbc
from azure.identity import DefaultAzureCredential
from flask import Flask, redirect, render_template, request, url_for

app = Flask(__name__)

SQL_CONNECTION_STRING = os.getenv("AZURE_SQL_CONNECTIONSTRING")

if not SQL_CONNECTION_STRING:
    raise RuntimeError("AZURE_SQL_CONNECTIONSTRING is not configured.")

LOCAL_SQL_MODE = os.getenv("LOCAL_SQL_MODE", "false").lower() == "true"


def get_sql_connection():
    if LOCAL_SQL_MODE:
        return pyodbc.connect(
            SQL_CONNECTION_STRING,
            timeout=30,
        )

    credential = DefaultAzureCredential()

    token = credential.get_token(
        "https://database.windows.net/.default"
    )

    token_bytes = token.token.encode("utf-16-le")

    access_token = struct.pack(
        f"<I{len(token_bytes)}s",
        len(token_bytes),
        token_bytes,
    )

    return pyodbc.connect(
        SQL_CONNECTION_STRING,
        attrs_before={1256: access_token},
        timeout=30,
    )


@app.route("/", methods=["GET"])
def index():
    todos = []

    with get_sql_connection() as connection:
        cursor = connection.cursor()

        cursor.execute(
            """
            SELECT Id, Title, Description, Completed
            FROM dbo.Todo
            ORDER BY Id DESC
            """
        )

        rows = cursor.fetchall()

        todos = [
            {
                "id": row.Id,
                "title": row.Title,
                "description": row.Description,
                "completed": bool(row.Completed),
            }
            for row in rows
        ]

    return render_template(
        "index.html",
        todos=todos,
    )


@app.route("/todos", methods=["POST"])
def create_todo():
    title = request.form.get("title", "").strip()
    description = request.form.get("description", "").strip()

    if not title:
        return "Title is required.", 400

    with get_sql_connection() as connection:
        cursor = connection.cursor()

        cursor.execute(
            """
            INSERT INTO dbo.Todo (Title, Description, Completed)
            VALUES (?, ?, 0)
            """,
            title,
            description,
        )

        connection.commit()

    return redirect(url_for("index"))


@app.route("/todos/<int:todo_id>/delete", methods=["POST"])
def delete_todo(todo_id):
    with get_sql_connection() as connection:
        cursor = connection.cursor()

        cursor.execute(
            """
            DELETE FROM dbo.Todo
            WHERE Id = ?
            """,
            todo_id,
        )

        connection.commit()

    return redirect(url_for("index"))


@app.route("/health", methods=["GET"])
def health():
    return {
        "status": "healthy",
        "service": "todo-python",
    }


if __name__ == "__main__":
    port = int(os.getenv("PORT", "8000"))

    app.run(
        host="0.0.0.0",
        port=port,
    )