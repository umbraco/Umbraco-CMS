namespace umbraco.models.search {
    export namespace base {
        export class searchBase {
            public menuUrl: string;
            public editorPath: string;
            public subTitle: string;
            public metaData: metaData;
        }

        export class metaData {
            public treeAlias: string;
            public Email: string;
        }
    }

    export class member extends base.searchBase {
        public id: number;
        public key: string;
    }
}