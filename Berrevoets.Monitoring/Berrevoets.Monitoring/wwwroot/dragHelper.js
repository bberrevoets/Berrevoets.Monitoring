function addDragClasses() {
    document.addEventListener("dragstart",
        function(e) {
            if (e.target.tagName === "TR") {
                e.target.classList.add("dragging");
            }
        });

    document.addEventListener("dragend",
        function(e) {
            if (e.target.tagName === "TR") {
                e.target.classList.remove("dragging");
            }
        });
}

window.dragHelper = {
    initialize: function() {
        document.addEventListener("dragstart",
            function(e) {
                if (e.target.tagName === "TR" || e.target.closest("tr")) {
                    console.log("JS: Drag start detected");
                    e.target.classList.add("dragging");
                    // Store the dragged element ID in dataTransfer
                    if (e.dataTransfer) {
                        e.dataTransfer.effectAllowed = "move";
                        e.dataTransfer.setData("text/plain", "dragging"); // Just a marker
                    }
                }
            });

        document.addEventListener("dragend",
            function(e) {
                if (e.target.tagName === "TR" || e.target.closest("tr")) {
                    e.target.classList.remove("dragging");
                }
            });

        document.addEventListener("dragover",
            function(e) {
                e.preventDefault(); // This is crucial for the drop event to fire
            },
            false);

        document.addEventListener("drop",
// ReSharper disable once UnusedParameter
            function(e) {
                console.log("JS: Drop detected");
            });
    }
};