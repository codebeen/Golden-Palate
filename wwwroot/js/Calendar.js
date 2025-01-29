
<script>
    // Event Listener for Button Click
    document.getElementById("btnChooseDate").addEventListener("click", function () {
        // Fetch the modal content dynamically
        fetch('@Url.Action("Calendar", "Reservation")')
            .then(response => response.text())
            .then(html => {
                // Inject the modal content into the container
                const modalContainer = document.getElementById("modalContainer");
                modalContainer.innerHTML = html;

                // Initialize and show the modal
                const modal = new bootstrap.Modal(document.getElementById("calendarModal"));
                modal.show();

                // Initialize the date picker
                $("#datepicker").datepicker();
            })
            .catch(error => console.error('Error loading modal:', error));
    });
</script>

$(document).ready(function () {
    // Initialize the jQuery UI Datepicker
    $("#datepicker").datepicker();

    // Handle the "Save Date" button click
    $("#saveDate").on("click", function () {
        const selectedDate = $("#datepicker").datepicker("getDate");
        if (selectedDate) {
            alert(`You selected: ${selectedDate.toLocaleDateString()}`);
        } else {
            alert("No date selected!");
        }
    });
});


document.addEventListener("click", function (event) {
    if (event.target && event.target.id === "confirmDate") {
        const selectedDate = document.getElementById("datepicker").value;
        if (selectedDate) {
            alert("You selected: " + selectedDate);
        } else {
            alert("Please select a date!");
        }
    }
});
