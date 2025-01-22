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

    try {
        const response = await fetch(`/api/quiz/category/${categoryId}`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json",
            },
        });

        const data = await response.json();
        console.log("API Response:", data);

        const quizzes = Array.isArray(data) ? data : [];
        const quiz = quizzes.find((q) => q.id == quizId);

        if (!quiz) {
            alert("Quiz not found!");
            window.location.href = `edit.html?categoryID=${categoryId}`;
            return;
        }

        const hasValidAnswers =
            quiz.answers && Array.isArray(quiz.answers) && quiz.answers.length > 0;

        if (
            hasValidAnswers &&
            quiz.answers.length === 2 &&
            quiz.answers.some((a) => a.description.toLowerCase() === "true")
        ) {
            trueFalseForm.style.display = "block";
            document.getElementById("description-tf").value = quiz.description || "";
            document.getElementById("true-false-answer").value =
                quiz.answers.find((a) => a.rightAnswer)?.description.toLowerCase() === "true"
                    ? "true"
                    : "false";
        } else {
            multipleChoiceForm.style.display = "block";
            document.getElementById("description-mc").value = quiz.description || "";

            if (hasValidAnswers) {
                const correctAnswer = quiz.answers.find((a) => a.rightAnswer)?.description || "";
                const incorrectAnswers = quiz.answers
                    .filter((a) => !a.rightAnswer)
                    .map((a) => a.description);

                document.getElementById("correct-answer").value = correctAnswer;
                document.getElementById("incorrect-answer-1").value = incorrectAnswers[0] || "";
                document.getElementById("incorrect-answer-2").value = incorrectAnswers[1] || "";
                document.getElementById("incorrect-answer-3").value = incorrectAnswers[2] || "";
            } else {
                document.getElementById("correct-answer").value = "";
                document.getElementById("incorrect-answer-1").value = "";
                document.getElementById("incorrect-answer-2").value = "";
                document.getElementById("incorrect-answer-3").value = "";
            }
        }

        if (quiz.media) {
            const mediaContainer = document.getElementById("existing-media-container");
            const existingMedia = document.getElementById("existing-media");

            mediaContainer.style.display = "block";
            if (quiz.media.mediaType === "video") {
                existingMedia.innerHTML = `<a href="https://www.youtube.com/watch?v=${quiz.media.filePath}" target="_blank">View YouTube Video</a>`;
            } else if (quiz.media.mediaType === "image") {
                existingMedia.innerHTML = `<img src="${quiz.media.filePath}" alt="Quiz Media" style="max-width: 100%;">`;
            }
        }
    } catch (error) {
        console.error("Error loading quiz data:", error);
        alert("Failed to load quiz data.");
    }

    multipleChoiceForm.addEventListener("submit", async function (event) {
        event.preventDefault();

        const description = document.getElementById("description-mc").value.trim();
        const correctAnswer = document.getElementById("correct-answer").value.trim();
        const incorrectAnswers = [
            document.getElementById("incorrect-answer-1").value.trim(),
            document.getElementById("incorrect-answer-2").value.trim(),
            document.getElementById("incorrect-answer-3").value.trim(),
        ];
        const mediaFile = document.getElementById("media-file-mc")?.files[0];
        const mediaType = document.getElementById("media-type-mc")?.value.trim();
        const youtubeLink = document.getElementById("youtube-link-mc").value.trim();

        if (!description || !correctAnswer || incorrectAnswers.some((answer) => !answer)) {
            alert("All fields must be filled out.");
            return;
        }

        const answers = [
            { Description: correctAnswer, RightAnswer: true },
            ...incorrectAnswers.map((answer) => ({ Description: answer, RightAnswer: false })),
        ];

        await submitQuizUpdate(quizId, categoryId, description, answers, mediaFile, mediaType, youtubeLink);
    });

    trueFalseForm.addEventListener("submit", async function (event) {
        event.preventDefault();

        const description = document.getElementById("description-tf").value.trim();
        const trueFalseAnswer = document.getElementById("true-false-answer").value === "true";
        const mediaFile = document.getElementById("media-file-tf")?.files[0];
        const mediaType = document.getElementById("media-type-tf")?.value.trim();
        const youtubeLink = document.getElementById("youtube-link-tf").value.trim();

        if (!description) {
            alert("Question description cannot be empty.");
            return;
        }

        const answers = [
            { Description: "True", RightAnswer: trueFalseAnswer },
            { Description: "False", RightAnswer: !trueFalseAnswer },
        ];

        await submitQuizUpdate(quizId, categoryId, description, answers, mediaFile, mediaType, youtubeLink);
    });

    const cancelBtns = document.querySelectorAll("#cancel-btn, #cancel-btn-tf");
    cancelBtns.forEach((btn) => {
        btn.addEventListener("click", () => {
            window.location.href = `edit.html?categoryID=${categoryId}`;
        });
    });

    async function submitQuizUpdate(quizId, categoryId, description, answers, mediaFile, mediaType, youtubeLink) {
        try {
            const formData = new FormData();
            formData.append("Description", description);
            formData.append("Answers", JSON.stringify(answers));

            if (mediaFile) {
                formData.append("MediaFile", mediaFile);
                formData.append("MediaType", mediaType);
            }

            if (youtubeLink && mediaType === "video") {
                formData.append("YouTubeLink", youtubeLink);
                formData.append("MediaType", "video");
            }

            const token = localStorage.getItem("authToken");
            const response = await fetch(`/api/quiz/update/${quizId}`, {
                method: "PUT",
                headers: {
                    "Authorization": `Bearer ${token}`,
                },
                body: formData,
            });

            if (response.ok) {
                alert("Quiz updated successfully!");
                window.location.href = `edit.html?categoryID=${categoryId}`;
            } else {
                const errorText = await response.text();
                alert(`Error: ${errorText}`);
            }
        } catch (error) {
            console.error("Error updating quiz:", error);
            alert("Failed to update quiz.");
        }
    }
});
