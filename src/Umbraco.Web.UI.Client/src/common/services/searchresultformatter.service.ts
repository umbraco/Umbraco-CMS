
namespace umbraco.services {
    export class searchResultFormatter {
        public constructor(protected umbRequestHelper) {

        }

        public configureDefaultResult(content, treeAlias, appAlias) {
            content.editorPath = appAlias + "/" + treeAlias + "/edit/" + content.id;
            angular.extend(content.metaData, { treeAlias: treeAlias });
        }

        public configureContentResult(content, treeAlias, appAlias) {
            content.menuUrl = this.umbRequestHelper.getApiUrl("contentTreeBaseUrl", "GetMenu", [{ id: content.id }, { application: appAlias }]);
            content.editorPath = appAlias + "/" + treeAlias + "/edit/" + content.id;
            angular.extend(content.metaData, { treeAlias: treeAlias });
            content.subTitle = content.metaData.Url;
        }

        public configureMemberResult(member: umbraco.models.search.member, treeAlias, appAlias) {
            member.menuUrl = this.umbRequestHelper.getApiUrl("memberTreeBaseUrl", "GetMenu", [{ id: member.id }, { application: appAlias }]);
            member.editorPath = appAlias + "/" + treeAlias + "/edit/" + (member.key ? member.key : member.id);
            angular.extend(member.metaData, { treeAlias: treeAlias });
            member.subTitle = member.metaData.Email;
        }

        public configureMediaResult(media, treeAlias, appAlias) {
            media.menuUrl = this.umbRequestHelper.getApiUrl("mediaTreeBaseUrl", "GetMenu", [{ id: media.id }, { application: appAlias }]);
            media.editorPath = appAlias + "/" + treeAlias + "/edit/" + media.id;
            angular.extend(media.metaData, { treeAlias: treeAlias });
        }
    }
}

angular.module('umbraco.services').services('searchResultFormatter', umbraco.services.searchSerice);