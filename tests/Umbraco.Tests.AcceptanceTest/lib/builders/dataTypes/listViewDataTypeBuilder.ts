import {DataTypeBuilder} from './dataTypeBuilder';
import {ListViewBulkActionPermissionsBuilder, ListViewLayoutBuilder, ListViewPropertyBuilder} from './listViewBuilder';

export class ListViewDataTypeBuilder extends DataTypeBuilder {
  pageSize: number;
  orderBy: string;
  orderDirection: string;
  layouts: ListViewLayoutBuilder[];
  includeProperties: ListViewPropertyBuilder[];
  bulkActionPermissions: ListViewBulkActionPermissionsBuilder;
  icon: string;
  tabName: string;
  showContentFirst: boolean;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.ListView';
    this.editorUiAlias = 'Umb.PropertyEditorUi.Collection';
    this.layouts = [];
    this.includeProperties = [];
    // this.bulkActionPermissions = new ListViewBulkActionPermissionsBuilder(this);
  }

  withPageSize(pageSize: number) {
    this.pageSize = pageSize;
    return this;
  }

  withOrderBy(orderBy: string) {
    this.orderBy = orderBy;
    return this;
  }

  withOrderDirection(orderDirection: string) {
    this.orderDirection = orderDirection;
    return this;
  }

  addLayout() {
    const builder = new ListViewLayoutBuilder(this);
    this.layouts.push(builder);
    return builder;
  }

  addColumnDisplayedProperty() {
    const builder = new ListViewPropertyBuilder(this);
    this.includeProperties.push(builder);
    return builder;
  }

  addBulkActionPermissions() {
    const builder = new ListViewBulkActionPermissionsBuilder(this);
    this.bulkActionPermissions = builder;
    return builder;
  }

  withIcon(icon: string) {
    this.icon = icon;
    return this;
  }

  withContentAppName(tabName: string) {
    this.tabName = tabName;
    return this;
  }

  withShowContentFirst(showContentFirst: boolean) {
    this.showContentFirst = showContentFirst;
    return this;
  }

  getValues() {
    let values: any = [];

    values.push({
      alias: 'pageSize',
      value: this.pageSize || 10
    });

    values.push({
      alias: 'orderBy',
      value: this.orderBy || 'updateDate'
    });

    values.push({
      alias: 'orderDirection',
      value: this.orderDirection || 'desc'
    });

    values.push({
      alias: 'layouts',
      value: this.layouts.map((builder) => {
        return builder.getValues();
      })
    });

    values.push({
      alias: 'includeProperties',
      value: this.includeProperties.map((builder) => {
        return builder.getValues();
      })
    });

    if (this.bulkActionPermissions !== undefined) {
      values.push({
        alias: 'bulkActionPermissions',
        value: this.bulkActionPermissions.getValues()
      });
    }

    values.push({
      alias: 'icon',
      value: this.icon || null
    });

    if (this.tabName !== undefined) {
      values.push({
        alias: 'tabName',
        value: this.tabName
      });
    }

    if (this.showContentFirst !== undefined) {
      values.push({
        alias: 'showContentFirst',
        value: this.showContentFirst
      });
    }
    return values;
  }
}