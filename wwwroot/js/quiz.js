document.addEventListener("DOMContentLoaded", async function () {
    const params = new URLSearchParams(window.location.search);
    const categoryID = params.get("categoryID");
    const autoplay = params.get("autoplay") === "true";
    const token = localStorage.getItem("authToken");

    if (!token) {
        alert("You must log in to access this page.");
        window.location.href = "login.html";
        return;
    }

    const quizContainer = document.getElementById("quiz-container");
    const questionText = document.getElementById("question-text");
    const answersContainer = document.getElementById("answers-container");
    const nextBtn = document.getElementById("next-btn");
    const quitBtn = document.getElementById("quit-btn");

    try {
        // Fetch quiz questions and answers
        const response = await fetch(`/api/quiz/category/${categoryID}`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("Failed to load quiz questions.");
        }

        console.log("response is ok");
        const questions = await response.json();  // Array of questions with answers
        console.log(questions);
        let currentQuestionIndex = 0;

        if (questions.length === 0) {
            alert("No questions available for this category.");
            window.location.href = "home.html";
            return;
        }

        // Display the first question
        displayQuestion(currentQuestionIndex, questions);

        if (autoplay) {
            // Autoplay mode
            let autoplayInterval = setInterval(() => {
                currentQuestionIndex++;
                if (currentQuestionIndex < questions.length) {
                    displayQuestion(currentQuestionIndex, questions);
                } else {
                    clearInterval(autoplayInterval);
                    alert("Quiz completed!");
                    window.location.href = "home.html";
                }
            }, 5000);  // Change question every 5 seconds
        } else {
            // Normal mode: Handle "Next" button
            nextBtn.style.display = "block";
            nextBtn.addEventListener("click", () => {
                currentQuestionIndex++;
                if (currentQuestionIndex < questions.length) {
                    displayQuestion(currentQuestionIndex, questions);
                } else {
                    alert("Quiz completed!");
                    window.location.href = "home.html";
                }
            });
        }

        quitBtn.addEventListener("click", () => {
            if (confirm("Are you sure you want to quit the quiz?")) {
                window.location.href = "home.html";
            }
        });

    } catch (error) {
        console.error("Error:", error);
        alert("Error loading quiz. Please try again.");
    }

    function displayQuestion(index, questions) {
        const question = questions[index];
        const questionText = document.getElementById("question-text");
        const answersContainer = document.getElementById("answers-container");

        questionText.innerText = `Q${index + 1}: ${question.description}`;
        answersContainer.innerHTML = "";  // Clear previous answers

        question.answers.forEach(answer => {
            const answerBtn = document.createElement("button");
            answerBtn.classList.add("answer-btn");
            answerBtn.innerText = answer.description;

            if (!autoplay) {
                // **Normal mode: User selects an answer manually**
                answerBtn.addEventListener("click", () => {
                    if (answer.isCorrect) {
                        answerBtn.style.backgroundColor = "green";  // Correct answer
                        alert("Correct answer!");
                    } else {
                        answerBtn.style.backgroundColor = "red";  // Incorrect answer
                        alert("Incorrect. Try again!");
                    }
                });
            }

            answersContainer.appendChild(answerBtn);
        });

        if (autoplay) {
            // **Autoplay mode: Highlight correct answer after 2 seconds**
            disableAllButtons();  // Disable user interaction
            setTimeout(() => {
                const correctAnswerBtn = Array.from(answersContainer.children).find(btn => {
                    return questions[index].answers.find(a => a.description === btn.innerText && a.isCorrect);
                });
                if (correctAnswerBtn) {
                    correctAnswerBtn.style.backgroundColor = "green";  // Highlight correct answer
                }
            }, 2000);  // Highlight correct answer after 2 seconds
        }
    }

    function disableAllButtons() {
        const answerButtons = document.querySelectorAll(".answer-btn");
        answerButtons.forEach(btn => {
            btn.disabled = true;  // Disable all buttons to prevent multiple clicks
        });
    }
});
