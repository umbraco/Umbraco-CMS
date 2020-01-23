/**
* @ngdoc service
* @name umbraco.services.variantHelper
* @description A helper service for dealing with variants
**/
function variantHelper() {
    /**
     * Returns the id for this variant
     * @param {any} variant
     */
    function getDisplayName(variant) {
        if (variant == null) {
            return "";
        }

        var parts = [];

        if (variant.language && variant.language.name) {
            parts.push(variant.language.name);
        }

        if (variant.segment) {
            var capitalized = variant.segment.split(" ").map(p => p[0].toUpperCase() + p.substring(1)).join(" ");
            parts.push(capitalized);
        }

        if (parts.length === 0) {
            // Invariant
            parts.push("Default");
        }

        return parts.join(" - ");
    }

    return {
        getDisplayName
    }
}
angular.module('umbraco.services').factory('variantHelper', variantHelper);
