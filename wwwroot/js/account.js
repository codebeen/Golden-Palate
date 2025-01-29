
function togglePassword() {
                                    var passwordField = document.getElementById('form3Example4');
    var eyeIcon = document.getElementById('eyeIcon');
    var showPasswordCheckbox = document.getElementById('showPassword');

    if (showPasswordCheckbox.checked) {
        passwordField.type = 'text'; // Show the password
        eyeIcon.classList.remove('fa-eye');
        eyeIcon.classList.add('fa-eye-slash'); // Change to eye-slash icon

    } else {
    passwordField.type = 'password'; // Hide the password
    eyeIcon.classList.remove('fa-eye-slash');
    eyeIcon.classList.add('fa-eye'); // Change to eye icon
    }
}

function validateForm() {
    var email = document.getElementById('form3Example3');
    var password = document.getElementById('form3Example4');
    var isValid = true;

    // Reset styles
    email.style.borderColor = '';
    password.style.borderColor = '';

    // Email validation
    if (!/^\S+\S+\.\S+$/.test(email.value.trim())) {
        isValid = false;
    email.style.borderColor = 'red';
    alert('Please enter a valid email address.');
                                    }

    // Password validation
    if (
    password.value.trim().length < 8 ||
    !/[A-Z]/.test(password.value) ||
    !/[a-z]/.test(password.value) ||
    !/[0-9]/.test(password.value)
    ) {
        isValid = false;
    password.style.borderColor = 'red';
    alert('Password must be at least 8 characters long and include uppercase, lowercase, and a number.');
    }

    return isValid; // Prevent form submission if validation fails
}

function togglePassword() {
    var passwordField = document.getElementById('form3Example4');
    var showPasswordCheckbox = document.getElementById('showPassword');

    if (showPasswordCheckbox.checked) {
        passwordField.type = 'text'; // Show the password
    } else {
        passwordField.type = 'password'; // Hide the password
    }
}

function togglePassword() {
    var passwordField = document.getElementById('form3Example4');
    var eyeIcon = document.getElementById('eyeIcon');
    var showPasswordCheckbox = document.getElementById('showPassword');

    if (showPasswordCheckbox.checked) {
        passwordField.type = 'text'; // Show the password
        eyeIcon.classList.remove('fa-eye');
        eyeIcon.classList.add('fa-eye-slash'); // Change to eye-slash icon
    } else {
        passwordField.type = 'password'; // Hide the password
        eyeIcon.classList.remove('fa-eye-slash');
        eyeIcon.classList.add('fa-eye'); // Change to eye icon
    }
}