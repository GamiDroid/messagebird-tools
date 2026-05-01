window.scrollToScheduleRow = function (tableId, rowIndex) {
    const table = document.getElementById(tableId);
    if (!table) return;
    // MudTable renders a thead (1 row) + tbody rows; skip header row
    const rows = table.querySelectorAll('tbody tr');
    const row = rows[rowIndex];
    if (row) {
        row.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
};
