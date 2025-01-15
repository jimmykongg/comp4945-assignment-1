document.addEventListener("DOMContentLoaded", async function () {
    const params = new URLSearchParams(window.location.search);
    const quizId = params.get("quizID");
    const categoryId = params.get("categoryID");
    const token = localStorage.getItem("authToken");

    if (!token) {
        alert("You must log in to access this page.");
        window.location.href = "login.html";
        return;
    }

    const multipleChoiceForm = document.getElementById("multiple-choice-form");
    const trueFalseForm = document.getElementById("true-false-form");

    // Load existing quiz data
    try {
        const response = await fetch(`/api/quiz/category/${categoryId}`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            }
        });

        const quizzes = await response.json();
        const quiz = quizzes.find(q => q.id == quizId);

        if (!quiz) {
            alert("Quiz not found!");
            window.location.href = `edit.html?categoryID=${categoryId}`;
            return;
        }

        // Populate form based on quiz type
        if (quiz.answers.length === 2 && quiz.answers.some(a => a.description.toLowerCase() === "true")) {
            // True/False Question
            trueFalseForm.style.display = "block";
            document.getElementById("description-tf").value = quiz.description;
            document.getElementById("true-false-answer").value = quiz.answers[0].isCorrect ? "true" : "false";
        } else {
            // Multiple Choice Question
            multipleChoiceForm.style.display = "block";
            document.getElementById("description-mc").value = quiz.description;
            document.getElementById("correct-answer").value = quiz.answers.find(a => a.isCorrect).description;
            const incorrectAnswers = quiz.answers.filter(a => !a.isCorrect).map(a => a.description);
            document.getElementById("incorrect-answer-1").value = incorrectAnswers[0];
            document.getElementById("incorrect-answer-2").value = incorrectAnswers[1];
            document.getElementById("incorrect-answer-3").value = incorrectAnswers[2];
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Failed to load quiz data.");
    }

    // Handle multiple-choice form submission
    multipleChoiceForm.addEventListener("submit", async function (event) {
        event.preventDefault();

        const description = document.getElementById("description-mc").value.trim();
        const correctAnswer = document.getElementById("correct-answer").value.trim();
        const incorrectAnswers = [
            document.getElementById("incorrect-answer-1").value.trim(),
            document.getElementById("incorrect-answer-2").value.trim(),
            document.getElementById("incorrect-answer-3").value.trim()
        ];

        if (!description) {
            alert("Question description cannot be empty.");
            return;
        }

        if (!correctAnswer || incorrectAnswers.some(answer => !answer)) {
            alert("All answer fields must be filled.");
            return;
        }

        const answers = [
            { Description: correctAnswer, RightAnswer: true },
            ...incorrectAnswers.map(answer => ({ Description: answer, RightAnswer: false }))
        ];

        await submitQuizUpdate(quizId, categoryId, description, answers);
    });

    // Handle true/false form submission
    trueFalseForm.addEventListener("submit", async function (event) {
        event.preventDefault();

        const description = document.getElementById("description-tf").value.trim();
        const trueFalseAnswer = document.getElementById("true-false-answer").value === "true";

        if (!description) {
            alert("Question description cannot be empty.");
            return;
        }

        const answers = [
            { Description: "True", RightAnswer: trueFalseAnswer },
            { Description: "False", RightAnswer: !trueFalseAnswer }
        ];

        await submitQuizUpdate(quizId, categoryId, description, answers);
    });

    // Handle cancel buttons
    document.getElementById("cancel-btn").addEventListener("click", () => {
        window.location.href = `edit.html?categoryID=${categoryId}`;
    });

    document.getElementById("cancel-btn-tf").addEventListener("click", () => {
        window.location.href = `edit.html?categoryID=${categoryId}`;
    });

    // Function to submit quiz update
    async function submitQuizUpdate(quizId, categoryId, description, answers) {
        try {
            const response = await fetch(`/api/quiz/update/${quizId}`, {
                method: "PUT",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ Description: description, Answers: answers })
            });

            if (response.ok) {
                alert("Quiz updated successfully!");
                window.location.href = `edit.html?categoryID=${categoryId}`;
            } else {
                const error = await response.text();
                alert(`Error: ${error}`);
            }
        } catch (error) {
            console.error("Error:", error);
            alert("Failed to update quiz.");
        }
    }
});
