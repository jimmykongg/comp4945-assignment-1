document.addEventListener("DOMContentLoaded", async function () {
    const params = new URLSearchParams(window.location.search);
    const categoryId = params.get("categoryID");
    const token = localStorage.getItem("authToken");

    if (!token) {
        alert("You must log in to access this page.");
        window.location.href = "login.html";
        return;
    }

    const quizList = document.getElementById("quiz-list");
    const uploadQuestionBtn = document.getElementById("upload-question-btn");
    const backToHomeBtn = document.getElementById("back-to-home-btn");

    // Redirect to home when "Back to Home" is clicked
    backToHomeBtn.addEventListener("click", () => {
        window.location.href = "home.html";
    });

    // Redirect to add quiz page when "Upload Question" is clicked
    uploadQuestionBtn.addEventListener("click", () => {
        window.location.href = `addQuiz.html?categoryID=${categoryId}`;
    });

    try {
        const response = await fetch(`/api/quiz/category/${categoryId}`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("Failed to load quiz questions.");
        }

        const quizzes = await response.json();  // Array of questions with answers
        if (quizzes.length === 0) {
            console.error("No quiz found.");
            quizList.innerHTML = "<p>No quizzes available for this category.</p>";
            return;
        }

        // Render each quiz as a card
        quizzes.forEach((quiz, index) => {
            console.log(quiz);
            const card = createQuizCard(quiz, index, categoryId);
            quizList.appendChild(card);
        });

    } catch (error) {
        console.error("Error:", error);
        alert("Error loading quizzes. Please try again.");
    }
});

function createQuizCard(quiz, index, categoryId) {
    const card = document.createElement("div");
    card.classList.add("quiz-card");

    card.innerHTML = `
        <h3>Q${index + 1}: ${quiz.description}</h3>
        <ul>
            ${quiz.answers.map(answer => `<li>${answer.description}</li>`).join("")}
        </ul>
        <div class="button-group">
            <button class="edit-btn" onclick="editQuiz(${quiz.id}, ${categoryId})">Edit</button>
            <button class="delete-btn" onclick="deleteQuiz(${quiz.id}, ${categoryId})">Delete</button>
        </div>
    `;
    return card;
}

async function editQuiz(quizId, categoryId) {
    window.location.href = `editQuiz.html?quizID=${quizId}&categoryID=${categoryId}`;
}

async function deleteQuiz(quizId, categoryId) {
    const confirmDelete = confirm("Are you sure you want to delete this quiz?");
    if (!confirmDelete) return;

    console.log("Deleting: " + quizId);
    const token = localStorage.getItem("authToken");

    try {
        const response = await fetch(`/api/quiz/delete/${quizId}`, {
            method: "DELETE",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            }
        });

        if (response.ok) {
            alert("Quiz deleted successfully!");
            window.location.href = `edit.html?categoryID=${categoryId}`;  // Reload the page to reflect changes
        } else {
            const error = await response.json();
            alert(`Error: ${error.message}`);
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Failed to delete quiz. Please try again.");
    }
}
