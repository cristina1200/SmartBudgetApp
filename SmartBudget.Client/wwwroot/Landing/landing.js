const slidesData = {
    dark: {
        logo: "Assets/logos/logo-dark.png",
        hero: "Assets/illustrations/login-dark.png",
        slides: [
            "Assets/illustrations/empty-transactions-dark.png",
            "Assets/illustrations/empty-savings-dark.png",
            "Assets/illustrations/no-data-dark.png",
            "Assets/illustrations/register-dark.png"
        ]
    },
    light: {
        logo: "Assets/logos/logo-light.png",
        hero: "Assets/illustrations/login.png",
        slides: [
            "Assets/illustrations/empty-transactions.png",
            "Assets/illustrations/empty-savings.png",
            "Assets/illustrations/no-data.png",
            "Assets/illustrations/register.png"
        ]
    }
};

let currentSlide = 0;

function getTheme() {
    return localStorage.getItem("theme") || "dark";
}

function applyTheme() {
    const theme = getTheme();
    const isLight = theme === "light";

    if (isLight) {
        document.body.classList.add("light-mode");
    } else {
        document.body.classList.remove("light-mode");
    }

    document.getElementById("themeIcon").className = isLight
        ? "fa-solid fa-sun"
        : "fa-solid fa-moon";

    document.getElementById("landingLogo").src = slidesData[theme].logo;
    document.getElementById("heroIllustration").src = slidesData[theme].hero;
    document.getElementById("slide1Image").src = slidesData[theme].slides[0];
    document.getElementById("slide2Image").src = slidesData[theme].slides[1];
    document.getElementById("slide3Image").src = slidesData[theme].slides[2];
    document.getElementById("slide4Image").src = slidesData[theme].slides[3];
}

function toggleTheme() {
    const nextTheme = getTheme() === "dark" ? "light" : "dark";
    localStorage.setItem("theme", nextTheme);
    applyTheme();
}

function renderDots() {
    const dotsContainer = document.getElementById("sliderDots");
    const totalSlides = document.querySelectorAll(".slide-card").length;

    dotsContainer.innerHTML = "";

    for (let i = 0; i < totalSlides; i++) {
        const dot = document.createElement("button");
        dot.className = `slider-dot ${i === currentSlide ? "active" : ""}`;
        dot.addEventListener("click", () => {
            currentSlide = i;
            updateSlider();
        });
        dotsContainer.appendChild(dot);
    }
}

function updateSlider() {
    const track = document.getElementById("sliderTrack");
    track.style.transform = `translateX(-${currentSlide * 100}%)`;

    document.querySelectorAll(".slider-dot").forEach((dot, index) => {
        dot.classList.toggle("active", index === currentSlide);
    });
}

function nextSlide() {
    const totalSlides = document.querySelectorAll(".slide-card").length;
    currentSlide = (currentSlide + 1) % totalSlides;
    updateSlider();
}

function prevSlide() {
    const totalSlides = document.querySelectorAll(".slide-card").length;
    currentSlide = (currentSlide - 1 + totalSlides) % totalSlides;
    updateSlider();
}

document.addEventListener("DOMContentLoaded", () => {
    applyTheme();
    renderDots();
    updateSlider();

    document.getElementById("themeToggle").addEventListener("click", toggleTheme);
    document.getElementById("nextSlide").addEventListener("click", nextSlide);
    document.getElementById("prevSlide").addEventListener("click", prevSlide);

    setInterval(nextSlide, 5000);
});