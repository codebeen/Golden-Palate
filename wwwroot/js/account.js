document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("form");
    const newPassword = document.getElementById("newPassword");
    const confirmPassword = document.getElementById("confirmPassword");
    const passwordError = document.getElementById("passwordError");
    const confirmError = document.getElementById("confirmError");

    const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@$%^&*()_+])[A-Za-z\d!@$%^&*()_+]{12,}$/;

    // Password Validation Function
    function validatePassword() {
        const passwordValue = newPassword.value.trim();
        const confirmValue = confirmPassword.value.trim();
        let isValid = true;

        // Validate new password strength
        if (!passwordPattern.test(passwordValue)) {
            passwordError.textContent = "Password must be at least 12 characters long, include uppercase, lowercase, a number, and a special character.";
            newPassword.classList.add("border-danger");
            isValid = false;
        } else {
            passwordError.textContent = "";
            newPassword.classList.remove("border-danger");
        }

        // Validate confirm password match
        if (confirmValue !== "" && passwordValue !== confirmValue) {
            confirmError.textContent = "Passwords do not match.";
            confirmPassword.classList.add("border-danger");
            isValid = false;
        } else {
            confirmError.textContent = "";
            confirmPassword.classList.remove("border-danger");
        }

        return isValid;
    }

    // Live validation on input change
    newPassword.addEventListener("input", validatePassword);
    confirmPassword.addEventListener("input", validatePassword);

    // Form submission validation
    form.addEventListener("submit", function (e) {
        if (!validatePassword()) {
            e.preventDefault();
        }
    });

    // Password Toggle Functionality
    function togglePassword(inputId, toggleId) {
        const input = document.getElementById(inputId);
        const toggleIcon = document.getElementById(toggleId);

        toggleIcon.addEventListener("click", function () {
            input.type = input.type === "password" ? "text" : "password";
            toggleIcon.classList.toggle("fa-eye");
            toggleIcon.classList.toggle("fa-eye-slash");
        });
    }

    togglePassword("currentPassword", "toggleCurrentPassword");
    togglePassword("newPassword", "toggleNewPassword");
    togglePassword("confirmPassword", "toggleConfirmPassword");
});



//function togglePassword() {
//                                    var passwordField = document.getElementById('form3Example4');
//    var eyeIcon = document.getElementById('eyeIcon');
//    var showPasswordCheckbox = document.getElementById('showPassword');

//    if (showPasswordCheckbox.checked) {
//        passwordField.type = 'text'; // Show the password
//        eyeIcon.classList.remove('fa-eye');
//        eyeIcon.classList.add('fa-eye-slash'); // Change to eye-slash icon

//    } else {
//    passwordField.type = 'password'; // Hide the password
//    eyeIcon.classList.remove('fa-eye-slash');
//    eyeIcon.classList.add('fa-eye'); // Change to eye icon
//    }
//}

//function validateForm() {
//    var email = document.getElementById('form3Example3');
//    var password = document.getElementById('form3Example4');
//    var isValid = true;

//    // Reset styles
//    email.style.borderColor = '';
//    password.style.borderColor = '';

//    // Email validation
//    if (!/^\S+\S+\.\S+$/.test(email.value.trim())) {
//        isValid = false;
//    email.style.borderColor = 'red';
//    alert('Please enter a valid email address.');
//                                    }

//    // Password validation
//    if (
//    password.value.trim().length < 8 ||
//    !/[A-Z]/.test(password.value) ||
//    !/[a-z]/.test(password.value) ||
//    !/[0-9]/.test(password.value)
//    ) {
//        isValid = false;
//    password.style.borderColor = 'red';
//    alert('Password must be at least 8 characters long and include uppercase, lowercase, and a number.');
//    }

//    return isValid; // Prevent form submission if validation fails
//}

//function togglePassword() {
//    var passwordField = document.getElementById('form3Example4');
//    var showPasswordCheckbox = document.getElementById('showPassword');

//    if (showPasswordCheckbox.checked) {
//        passwordField.type = 'text'; // Show the password
//    } else {
//        passwordField.type = 'password'; // Hide the password
//    }
//}

//function togglePassword() {
//    var passwordField = document.getElementById('form3Example4');
//    var eyeIcon = document.getElementById('eyeIcon');
//    var showPasswordCheckbox = document.getElementById('showPassword');

//    if (showPasswordCheckbox.checked) {
//        passwordField.type = 'text'; // Show the password
//        eyeIcon.classList.remove('fa-eye');
//        eyeIcon.classList.add('fa-eye-slash'); // Change to eye-slash icon
//    } else {
//        passwordField.type = 'password'; // Hide the password
//        eyeIcon.classList.remove('fa-eye-slash');
//        eyeIcon.classList.add('fa-eye'); // Change to eye icon
//    }
//}

