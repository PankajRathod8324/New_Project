@model DAL.ViewModel.MenuCategoryVM

@{
    Layout = "_LayoutOrder";
    ViewData["Title"] = "Table View";
}

<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Pizza Shop</title>

    <!-- Bootstrap CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons/font/bootstrap-icons.css">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery-validate/1.19.5/jquery.validate.min.js"></script>
    <script
        src="https://cdnjs.cloudflare.com/ajax/libs/jquery-validation-unobtrusive/3.2.12/jquery.validate.unobtrusive.min.js"></script>


</head>

<body class="" style="background-color: rgb(245,245,245);">
    <div style="padding-top: 100px; padding-left: 50px; padding-right: 50px;">
        <div class="container-fluid">
            <div class="row">
                <div class="col">
                    <div class="fs-2 fw-bold webybtn mb-3">KOT </div>
                </div>
            </div>
        </div>

        <ul class="nav nav-tabss border-0 bg-white p-3 pt-2 pb-0 rounded-3">
            @foreach (var category in Model.menuCategories)
            {
                <li class="nav-item">
                    <a class="nav-link active text-primary fs-5 fw-bold border-bottom border-primary" data-bs-toggle="tab"
                        href="#tab1">@category.CategoryName</a>
                </li>
            }
        </ul>

        <div class="tab-content mt-3 bg-white">
            <div class="tab-pane fade show active" id="tab1">
                <p>Content for Tab 1.</p>
            </div>
            <div class="tab-pane fade" id="tab2">
                <p>Content for Tab 2.</p>
            </div>
            <div class="tab-pane fade" id="tab3">
                <p>Content for Tab 3.</p>
            </div>
        </div>



    </div>



</body>

<script>
    document.querySelectorAll('.nav-link').forEach(tab => {
        tab.addEventListener('click', function () {
            document.querySelectorAll('.nav-link').forEach(t => {
                t.classList.remove('text-primary', 'border-bottom', 'border-primary');
                t.classList.add('text-dark');
            });
            this.classList.add('text-primary', 'border-bottom', 'border-primary');
            this.classList.remove('text-dark');
        });
    });
</script>

</html>

@section Scripts {
    @{
        await Html.RenderPartialAsync("_ValidationScriptsPartial");
    }
}
