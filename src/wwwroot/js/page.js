$(function () {
    var productsServiceUrl = 'api/products';
    
    // Cloud Animation
    $('#far-clouds1').pan({ fps: 30, speed: 0.7, dir: 'left', depth: 30 });
    $('#near-clouds1').pan({ fps: 30, speed: 1, dir: 'left', depth: 70 });
    $('#far-clouds2').pan({ fps: 30, speed: 0.7, dir: 'left', depth: 100 });
    $('#near-clouds2').pan({ fps: 30, speed: 1, dir: 'left', depth: 130 });

    // Read current data and rebuild UI.
    // If you plan to generate complex UIs like this, consider using a JavaScript templating library.
    function refreshProducts() {
        $('#summary').html('Retrieving products...');
        $.ajax({
            url: productsServiceUrl,
            type: 'GET',
            dataType: 'json',
            tryCount: 0,
            retryLimit: 3,
            success: function (data) {
                if (Array.isArray(data) && data.length > 0) {
                    var products = $.map(data, function (product) {
                        return $('<tr>').append($('<td><input class="product-text" style="width: 70px" value="' + product.productId + '"/></td>'))
                            .append($('<td><input class="product-text" value="' + product.name + '"/></td>'))
                            .append($('<td><input class="product-text" value="' + product.category + '"/></td>'))
                            .append($('<td><input class="product-text" style="width: 100px" value="' + parseFloat(product.price).toFixed(2) + '"/></td>'))
                            .append($('<td><button class="button-update" style="width: 70px" >Update</button></td>'))
                            .append($('<td><button class="button-delete" style="width: 70px" >Delete</button></td>')).append('</tr>');
                    });
                    $('#products > tbody').empty().append(products).toggle(products.length > 0);
                    $('#summary').html('<strong>' + products.length + '</strong> product(s) successfully retrieved.');
                } else {
                    $('#summary').html('No product retrieved.');
                }
            },
            error: function (xhr, textStatus, error) {
                if (textStatus == 'timeout') {
                    this.tryCount++;
                    if (this.tryCount <= this.retryLimit) {
                        //try again
                        $.ajax(this);
                        return;
                    }
                    return;
                }
                var errorMessage = JSON.parse(xhr.responseText);
                var text = errorMessage.error;
                $('#errorlog').append($('<li>').text(text));
            }
        });
    }

    // Handle insert
    $('#add-product').submit(function(event) {
        if ($('#product-name').val() === '') {
            $('#summary').html('The <strong>Name</strong> field cannot be null.');
            event.preventDefault();
            return;
        }
        if ($('#product-category').val() === '') {
            $('#summary').html('The <strong>Category</strong> field cannot be null.');
            event.preventDefault();
            return;
        }
        if ($('#product-price').val() === '') {
            $('#summary').html('The <strong>Price</strong> field cannot be null.');
            event.preventDefault();
            return;
        }
        var product = {
            'productId': 0,
            'name': $('#product-name').val(),
            'category': $('#product-category').val(),
            'price': parseFloat($('#product-price').val())
        };
        var data = JSON.stringify(product);
        $('#summary').html('Adding <strong>' + product.name + '</strong>...');
        $.ajax({
            url: productsServiceUrl,
            type: 'POST',
            cache: false,
            data: data,
            dataType: 'json',
            tryCount: 0,
            retryLimit: 3,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                if (data) {
                    var row = $('<tr>').append($('<td><input class="product-text" value="' + data.productId + '"/></td>'))
                        .append($('<td><input class="product-text" value="' + data.name + '"/></td>'))
                        .append($('<td><input class="product-text" value="' + data.category + '"/></td>'))
                        .append($('<td><input class="product-text" value="' + data.price + '"/></td>'))
                        .append($('<td><button class="button-update">Update</button></td>'))
                        .append($('<td><button class="button-delete">Delete</button></td>')).append('</tr>');
                    $('#products > tbody').append(row).toggle(true);
                    $('#summary').html('<strong>' + data.name + '</strong> successfully added.');
                }
            },
            error: function (xhr, textStatus, error) {
                if (textStatus == 'timeout') {
                    this.tryCount++;
                    if (this.tryCount <= this.retryLimit) {
                        //try again
                        $.ajax(this);
                        return;
                    }
                    return;
                }
                var errorMessage = JSON.parse(xhr.responseText);
                var text = errorMessage.error;
                $('#errorlog').append($('<li>').text(text));
            }
        });
        $('#product-name').val('');
        $('#product-category').val('');
        $('#product-price').val('');
        event.preventDefault();
    });

    // Handle update
    $(document.body).on('click', '.button-update', function (event) {
        var row = $(this).closest('tr');
        var productId = row.find('td:nth-child(1) input').val();
        if (!productId) {
            $('#summary').html('The <strong>product id</strong> field cannot be null.');
            event.preventDefault();
            return;
        }
        var name = row.find('td:nth-child(2) input').val();
        if (!name) {
            $('#summary').html('The <strong>name</strong> field cannot be null.');
            event.preventDefault();
            return;
        }
        var category = row.find('td:nth-child(3) input').val();
        if (!category) {
            $('#summary').html('The <strong>category</strong> field cannot be null.');
            event.preventDefault();
            return;
        }
        var price = row.find('td:nth-child(4) input').val();
        if (!price) {
            $('#summary').html('The <strong>price</strong> field cannot be null.');
            event.preventDefault();
            return;
        }
        var product = {
            'productId': parseInt(productId),
            'name': name,
            'category': category,
            'price': parseFloat(price)
        };
        var data = JSON.stringify(product);
        $('#summary').html('Updating <strong>' + product.name + '</strong>...');
        $.ajax({
            url: productsServiceUrl + '/' + productId,
            type: 'PUT',
            cache: false,
            data: data,
            dataType: 'json',
            tryCount: 0,
            retryLimit: 3,
            contentType: 'application/json; charset=utf-8',
            success: function () {
                $('#summary').html('<strong>' + product.name + '</strong> successfully updated.');
            },
            error: function (xhr, textStatus, error) {
                if (textStatus == 'timeout') {
                    this.tryCount++;
                    if (this.tryCount <= this.retryLimit) {
                        //try again
                        $.ajax(this);
                        return;
                    }
                    return;
                }
                var errorMessage = JSON.parse(xhr.responseText);
                var text = errorMessage.error;
                $('#errorlog').append($('<li>').text(text));
            }
        });
        event.preventDefault();
    });

    // Handle delete
    $(document.body).on('click', '.button-delete', function (event) {
        var productId = $(this).closest('tr').find('td:nth-child(1) input').val();
        var name = $(this).closest('tr').find('td:nth-child(2) input').val();
        var row = $(this).parents('tr').first();
        if (productId) {
            $('#summary').html('Deleting <strong>' + name + '</strong>...');
            $.ajax({
                url: productsServiceUrl + '/' + productId,
                type: 'DELETE',
                dataType: 'json',
                tryCount: 0,
                retryLimit: 3,
                success: function () {
                    row.remove();
                    $('#summary').html('<strong>' + name + '</strong> successfully deleted.');
                },
                error: function (xhr, textStatus, error) {
                    if (textStatus == 'timeout') {
                        this.tryCount++;
                        if (this.tryCount <= this.retryLimit) {
                            //try again
                            $.ajax(this);
                            return;
                        }
                        return;
                    }
                    var errorMessage = JSON.parse(xhr.responseText);
                    var text = errorMessage.error;
                    $('#errorlog').append($('<li>').text(text));
                }
            });
            event.preventDefault();
        }
    });

    // Handle refresh
    $(document.body).on('click', '.button-refresh', function (event) {
        refreshProducts();
        event.preventDefault();
        $('#errorlog').body = '';
    });

    // On initial load, start by fetching the current data
    refreshProducts();
});