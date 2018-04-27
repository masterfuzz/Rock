﻿var contentSlug = function () {
    var _selectors = {
        btnAdd: '#lbAdd',
        slugSection: '.js-slugs',
        slugRow: '.js-slug-row',
        btnSlugSave: '.js-slug-save',
        inputSlug: '.js-slug-input',
        slugId: '.js-slug-id',
        inputGroup: '.input-slug-group',
        slugLiteral: '.js-slug-literal',
        btnEdit: '.js-slug-edit',
        btnDelete: '.js-slug-remove'
    };

    var _contentSlugSelector;
    var _contentChannelItemSelector;
    var _saveSlug;
    var _uniqueSlug;
    var _removeSlug;

    function init(settings) {
        contentChannelItemSelector = $(settings.contentChannelItem);
        _saveSlug = settings.SaveSlug;
        _uniqueSlug = settings.UniqueSlug;
        _contentSlugSelector = settings.contentSlug;
        _removeSlug = settings.RemoveSlug;
        subscribeToEvents();
    }
    function subscribeToEvents() {
        $(_selectors.btnAdd).unbind('click');
        $(_selectors.btnAdd).click(function (e) {
            e.preventDefault();
            e.stopPropagation();
            var html = '<div class="row margin-l-sm margin-b-sm rollover-container js-slug-row clearfix">' +
                '<input id="slugId" class="js-slug-id" type="hidden" value="" />' +
                '<div class="input-group input-slug-group">' +
                '<input class="form-control js-slug-input" />' +
                '<span class="input-group-addon">' +
                '<a class="js-slug-save" href="#"><i class="fa fa-check"></i></a>' +
                '</span>' +
                '</div >' +
                '</div >';
            $(_selectors.btnAdd).before(html);
            if ($(contentChannelItemSelector).val() === "0") {
                $(_selectors.btnAdd).hide();
            }
            subscribeToEvents();
        });

        $(_selectors.btnSlugSave).unbind('click');
        $(_selectors.btnSlugSave).click(function (e) {
            e.preventDefault();
            e.stopPropagation();
            var row = $(this).closest(_selectors.slugRow);
            var inputSlug = row.find(_selectors.inputSlug).val();
            var slugId = row.find(_selectors.slugId).val();
            if (inputSlug !== '') {
                if ($(contentChannelItemSelector).val() === "0") {
                    uniqueSlug(inputSlug, row);
                } else {
                    saveSlug(inputSlug, slugId, row);
                }
            }
        });

        $(_selectors.btnEdit).unbind('click');
        $(_selectors.btnEdit).click(function (e) {
            e.preventDefault();
            e.stopPropagation();
            var row = $(this).closest(_selectors.slugRow);
            var slug = row.find(_selectors.slugLiteral).html();
            setSlugEdit(slug,row);
        });

        $(_selectors.btnDelete).unbind('click');
        $(_selectors.btnDelete).click(function (e) {
            e.preventDefault();
            e.stopPropagation();
            var row = $(this).closest(_selectors.slugRow);
            var slugId = row.find(_selectors.slugId).val();
            if (slugId !== '') {
                removeSlug(slugId, row);
            } else {
                $(_contentSlugSelector).val('');
                $(row).remove();
                $(_selectors.btnAdd).show();
            }
        });
    }

    function removeSlug(slugId, row) {
        $.ajax({
            url: _removeSlug.restUrl + _removeSlug.restParams.replace('{id}', slugId),
            type: 'DELETE',
            dataType: 'json',
            contentType: 'application/json'
        })
            .done(function (data) {
                $(row).remove();
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
            });
        
    }
    function setSlugEdit(slug, row) {

        $(row).children().not(_selectors.slugId).remove();
        var html =
            '<div class="input-group input-slug-group">' +
            '<input class="form-control js-slug-input" Value="'+slug+'" />' +
            '<span class="input-group-addon">' +
            '<a class="js-slug-save" href="#"><i class="fa fa-check"></i></a>' +
            '</span>' +
            '</div >';
        $(row).find(_selectors.slugId).after(html);
        subscribeToEvents();
    }
    function setSlugDetail(slug, slugId, row) {
        $(row).find(_selectors.slugId).val(slugId);
        if (slugId === '') {
            $(_contentSlugSelector).val(slug);
        }
        $(row).find(_selectors.inputGroup).remove();
        var html = '<literal class="js-slug-literal">' + slug + '</literal>' +
            '<div class="rollover-item actions pull-right">' +
            '<a class="js-slug-edit" href="#"><i class="fa fa-pencil"></i></a>' +
            '<a class="js-slug-remove" href="#"><i class="fa fa-close"></i></a>' +
            '</div >';
        $(row).find(_selectors.slugId).after(html);
        subscribeToEvents();
    }

    function saveSlug(slug, slugId, row) {
        $.ajax({
            url: _saveSlug.restUrl + _saveSlug.restParams.replace('{slug}', encodeURI($.trim(slug))).replace('{contentChannelItemSlugId?}', slugId),
            type: 'POST',
            dataType: 'json',
            contentType: 'application/json'
        })
            .done(function (data) {
                setSlugDetail(data.Slug, data.Id, row);
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
            });
    }
    function uniqueSlug(slug, row) {
        $.ajax({
            url: _uniqueSlug.restUrl + _uniqueSlug.restParams.replace('{slug}', slug),
            dataType: 'json',
            contentType: 'application/json'
        })
            .done(function (data) {
                setSlugDetail(data, '', row);
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
            });
    }
    return {
        init: init
    };

}();