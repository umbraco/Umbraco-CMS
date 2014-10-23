function xmlDataIntegrityReportController($scope, umbRequestHelper, $log, $http, $q, $timeout) {

    function check(item) {
        var action = item.check;
        umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("xmlDataIntegrityBaseUrl", action)),
                'Failed to retrieve data integrity status')
            .then(function(result) {
                item.checking = false;
                item.invalid = result === "false";
            });
    }

    $scope.fix = function(item) {
        var action = item.fix;
        if (item.fix) {
            if (confirm("This will cause all xml structures for this type to be rebuilt. " +
                "Depending on how much content there is in your site this could take a while. " +
                "It is not recommended to rebuild xml structures if they are not out of sync, during times of high website traffic " +
                "or when editors are editing content.")) {
                item.fixing = true;
                umbRequestHelper.resourcePromise(
                    $http.post(umbRequestHelper.getApiUrl("xmlDataIntegrityBaseUrl", action)),
                    'Failed to retrieve data integrity status')
                .then(function (result) {
                    item.fixing = false;
                    item.invalid = result === "false";
                });
            }
        }
    }

    $scope.items = {
        "contentXml": {
            label: "Content in the cmsContentXml table",
            checking: true,
            fixing: false,
            fix: "FixContentXmlTable",
            check: "CheckContentXmlTable"
        },
        "mediaXml": {
            label: "Media in the cmsContentXml table",
            checking: true,
            fixing: false,
            fix: "FixMediaXmlTable",
            check: "CheckMediaXmlTable"
        },
        "memberXml": {
            label: "Members in the cmsContentXml table",
            checking: true,
            fixing: false,
            fix: "FixMembersXmlTable",
            check: "CheckMembersXmlTable"
        }
    };

    for (var i in $scope.items) {
        check($scope.items[i]);
    }

}
angular.module("umbraco").controller("Umbraco.Dashboard.XmlDataIntegrityReportController", xmlDataIntegrityReportController);