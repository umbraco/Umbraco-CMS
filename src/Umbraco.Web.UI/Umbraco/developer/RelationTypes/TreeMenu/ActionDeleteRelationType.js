function actionDeleteRelationType(relationTypeId, relationTypeName) {

    if (confirm('Are you sure you want to delete "' + relationTypeName + '"?')) {
        $.ajax({
            type: "POST",
            url: "developer/RelationTypes/RelationTypesWebService.asmx/DeleteRelationType",
            data: "{ 'relationTypeId' : '" + relationTypeId + "' }",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                UmbClientMgr.mainTree().refreshTree('relationTypes');
                UmbClientMgr.appActions().openDashboard('developer');
            },
            error: function (data) { }
        });

    }
   
}


