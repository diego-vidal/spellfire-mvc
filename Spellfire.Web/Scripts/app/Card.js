﻿var Spellfire = window.Spellfire || {};

Spellfire.Card =
    (function (module, $) {

        "use strict";

        // Public
        module.init = function () {

            self.initilizeVariables();
            self.attachHandlers();

            self.$search.click();
        };

        // Private
        var self = {

            initilizeVariables: function () {

                self.$logo = $("#logo");
                self.$container = $("#container");
                self.$includeOnlineBoosters = $("#includeOnlineBoosters");
                self.$includeOnlineBoostersLabel = $("#includeOnlineBoostersLabel");
                self.$search = $("#search");
                self.$searchText = $("#SearchText");
                self.$errorMessage = $("#errorMessage");
                self.$cardList = $("#cardList");
                self.$cardDetail = $("#cardDetail");

                self.getStoredClientValues();
            },

            attachHandlers: function () {

                self.$logo.on("click", self.redirectHome);
                self.$search.click(self.getCardList);
                self.$includeOnlineBoostersLabel.click(function () {
                    self.$includeOnlineBoosters.trigger("click");
                });
                self.$cardList.on("click", "a.selectable", self.getCardDetails);
                self.$searchText.keypress(self.searchOnEnter);
                self.$searchText.click(function () {
                    $(this).select();
                });
            },

            redirectHome: function () {
                window.location.href = "/";
            },

            getStoredClientValues: function () {

                var clientValues = amplify.store();

                if (clientValues.searchText) {
                    self.$searchText.val(clientValues.searchText);
                }

                if (clientValues.includeOnlineBoosters) {
                    self.$includeOnlineBoosters.prop('checked', clientValues.includeOnlineBoosters);
                }
            },

            displayError: function (message) {

                if (message) {
                    self.$errorMessage.html(message);
                    self.$errorMessage.removeClass("hidden");
                }
            },

            
            getCardList: function () {

                var searchText = self.$searchText.val();

                if (!searchText) {
                    return;
                }

                var includeOnlineBoosters = self.$includeOnlineBoosters.is(":checked");

                amplify.store("searchText", searchText);
                amplify.store("includeOnlineBoosters", includeOnlineBoosters);

                self.$cardDetail.html("");

                Spellfire.Notification.show();

                $.ajax({
                    type: "GET",
                    url: "Card/List",
                    data: { searchText: searchText, includeOnlineBoosters: includeOnlineBoosters },
                    cache: true
                })
                .done(function (result) {

                    if (result.hasMessage) {

                        self.displayError(result.message);
                        return;
                    }

                    self.$cardList.html(result);
                    self.selectFirstResult();
                })
                .fail(function (jqXHR, textStatus, errorThrown) {
                    self.displayError(errorThrown);
                })
                .always(function () {

                    Spellfire.Notification.hide();
                });
            },

            getCardDetails: function () {

                var sequence = $(this).data("sequence");

                $.ajax({
                    type: "GET",
                    url: "Card/Details/",
                    data: { id: sequence },
                    datatype: 'json',
                    cache: false
                })
                .done(function (result) {
                    
                    if (result.hasMessage) {

                        self.displayError(result.message);
                        return;
                    }

                    self.$cardDetail.html(result);
                })
                .fail(function (jqXHR, textStatus, errorThrown) {
                    self.displayError(errorThrown);
                })
                .always(function () {
                    Spellfire.Notification.hide();
                });
            },

            searchOnEnter: function (e) {

                var code = e.keycode ? e.keycode : e.which;

                if (code == 13) { //ENTER
                    e.preventDefault();
                    self.$search.click();
                }
            },

            selectFirstResult: function () {

                $("#cardResults .selectable").first().click();
            }
        };

        return module;

    })(Spellfire.Card || {}, window.jQuery);
