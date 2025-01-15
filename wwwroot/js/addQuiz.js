document.addEventListener("DOMContentLoaded", function () {
    const params = new URLSearchParams(window.location.search);
    const categoryId = params.get("categoryID");
    const token = localStorage.getItem("authToken");
    const form = document.getElementById("add-quiz-form");
    const questionTypeSelect = document.getElementById("question-type");
    const multipleChoiceOptions = document.getElementById("multiple-choice-options");
    const trueFalseOptions = document.getElementById("true-false-options");

    // Toggle question type options
    questionTypeSelect.addEventListener("change", function () {
        if (this.value === "multiple-choice") {
            multipleChoiceOptions.style.display = "block";
            trueFalseOptions.style.display = "none";
        } else {
            multipleChoiceOptions.style.display = "none";
            trueFalseOptions.style.display = "block";
        }
    });

    // Handle form submission
    form.addEventListener("submit", async function (event) {
        event.preventDefault();  // Prevent page reload
        const questionType = questionTypeSelect.value;
        const description = document.getElementById("description").value.trim();
        let answers = [];

        if (!description) {
            alert("Question description cannot be empty.");
            return;
        }

        if (questionType === "multiple-choice") {
            // Collect multiple choice answers
            const correctAnswer = document.getElementById("correct-answer").value.trim();
            const incorrectAnswers = [
                document.getElementById("incorrect-answer-1").value.trim(),
                document.getElementById("incorrect-answer-2").value.trim(),
                document.getElementById("incorrect-answer-3").value.trim()
            ];

            if (!correctAnswer || incorrectAnswers.some(answer => !answer)) {
                alert("All answer fields must be filled.");
                return;
            }

            answers.push({ Description: correctAnswer, RightAnswer: true });
            incorrectAnswers.forEach(answer => answers.push({ Description: answer, RightAnswer: false }));
        } else {
            // Collect true/false answer
            const trueFalseAnswer = document.getElementById("true-false-answer").value === "true";
            answers.push({ Description: "True", RightAnswer: trueFalseAnswer });
            answers.push({ Description: "False", RightAnswer: !trueFalseAnswer });
        }

        console.log("Request Body:")
        console.log(categoryId)
        console.log(description)
        console.log(answers);
        // Send POST request to backend
        try {
            const response = await fetch(`/api/quiz/add/${categoryId}`, {
                method: "POST",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ Description: description, Answers: answers })
            });

            if (response.ok) {
                alert("Quiz question added successfully!");
                window.location.href = `edit.html?categoryID=${categoryId}`;
            } else {
                const error = await response.text();
                alert(`Error: ${error}`);
            }
        } catch (error) {
            console.error("Error:", error);
            alert("Failed to add quiz question. Please try again.");
        }
    });

    // Handle cancel button
    document.getElementById("cancel-btn").addEventListener("click", () => {
        window.location.href = `edit.html?categoryID=${categoryId}`;
    });
});
