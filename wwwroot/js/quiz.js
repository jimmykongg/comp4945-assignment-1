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
    const mediaContainer = document.getElementById("media-container");
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

        const questions = await response.json();
        let currentQuestionIndex = 0;

        if (questions.length === 0) {
            alert("No questions available for this category.");
            window.location.href = "home.html";
            return;
        }

        // Display the first question
        displayQuestion(currentQuestionIndex, questions);

        if (autoplay) {
            let autoplayInterval = setInterval(() => {
                currentQuestionIndex++;
                if (currentQuestionIndex < questions.length) {
                    displayQuestion(currentQuestionIndex, questions);
                } else {
                    clearInterval(autoplayInterval);
                    alert("Quiz completed!");
                    window.location.href = "home.html";
                }
            }, 5000);
        } else {
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
        questionText.innerText = `Q${index + 1}: ${question.description}`;
        answersContainer.innerHTML = ""; // Clear previous answers
        mediaContainer.innerHTML = ""; // Clear previous media content

        question.answers.forEach(answer => {
            const answerBtn = document.createElement("button");
            answerBtn.classList.add("answer-btn");
            answerBtn.innerText = answer.description;

            if (!autoplay) {
                answerBtn.addEventListener("click", () => {
                    if (answer.rightAnswer) {
                        answerBtn.style.backgroundColor = "green"; // Correct answer
                        alert("Correct answer!");
                    } else {
                        answerBtn.style.backgroundColor = "red"; // Incorrect answer
                        alert("Incorrect. Try again!");
                    }
                });
            }

            answersContainer.appendChild(answerBtn);
        });

        if (question.media) {
            const { mediaType, filePath } = question.media;

            mediaContainer.style.display = "block";

            if (mediaType === "image") {
                const img = document.createElement("img");
                img.src = filePath;
                img.alt = "Quiz Media";
                img.style.maxWidth = "100%";
                mediaContainer.appendChild(img);
            } else if (mediaType === "video") {
                const iframe = document.createElement("iframe");
                iframe.src = `https://www.youtube.com/embed/${filePath}`;
                iframe.width = "560";
                iframe.height = "315";
                iframe.title = "YouTube video player";
                iframe.frameBorder = "0";
                iframe.allow = "accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture";
                iframe.allowFullscreen = true;
                mediaContainer.appendChild(iframe);
            }
        } else {
            mediaContainer.style.display = "none";
        }

        if (autoplay) {
            disableAllButtons();
            setTimeout(() => {
                const correctAnswerBtn = Array.from(answersContainer.children).find(btn => {
                    return questions[index].answers.find(a => a.description === btn.innerText && a.rightAnswer);
                });
                if (correctAnswerBtn) {
                    correctAnswerBtn.style.backgroundColor = "green";
                }
            }, 2000);
        }
    }

    function disableAllButtons() {
        const answerButtons = document.querySelectorAll(".answer-btn");
        answerButtons.forEach(btn => {
            btn.disabled = true;
        });
    }
});
