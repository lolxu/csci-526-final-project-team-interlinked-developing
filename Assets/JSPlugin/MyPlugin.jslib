var MyPlugin = {
    Initialize: function() {
        window.onbeforeunload = function() {
           return "Are you sure you really want to leave the application?";
        };
    }
};

mergeInto(LibraryManager.library, MyPlugin);