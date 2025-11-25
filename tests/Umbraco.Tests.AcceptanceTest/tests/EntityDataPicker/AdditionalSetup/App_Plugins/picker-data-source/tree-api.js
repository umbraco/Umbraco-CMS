import {UmbControllerBase} from "@umbraco-cms/backoffice/class-api";

export class MyPickerDataSource extends UmbControllerBase {
  treePickableFilter = (treeItem) =>
    !!treeItem.unique && treeItem.entityType === "example";

  searchPickableFilter = (searchItem) =>
    !!searchItem.unique && searchItem.entityType === "example";

  async requestTreeRoot() {
    const root = {
      unique: null,
      name: "Examples",
      icon: "icon-folder",
      hasChildren: true,
      entityType: "example-root",
      isFolder: true,
    };

    return {data: root};
  }

  async requestTreeRootItems() {
    const rootItems = customItems.filter((item) => item.parent.unique === null);

    const data = {
      items: rootItems,
      total: rootItems.length,
    };

    return {data};
  }

  async requestTreeItemsOf(args) {
    const items = customItems.filter(
      (item) =>
        item.parent.entityType === args.parent.entityType &&
        item.parent.unique === args.parent.unique
    );

    const data = {
      items: items,
      total: items.length,
    };

    return {data};
  }

  async requestTreeItemAncestors() {
    // TODO: implement when needed
    return {data: []};
  }

  async requestItems(uniques) {
    const items = customItems.filter((x) => uniques.includes(x.unique));
    return {data: items};
  }

  async search(args) {
    const result = customItems.filter((item) =>
      item.name.toLowerCase().includes(args.query.toLowerCase())
    );

    const data = {
      items: result,
      total: result.length,
    };

    return {data};
  }
}

export {MyPickerDataSource as api};

const customItems = [
  {
    unique: "1",
    entityType: "example",
    name: "Example 1",
    icon: "icon-shape-triangle",
    parent: {unique: null, entityType: "example-root"},
    isFolder: false,
    hasChildren: false,
  },
  {
    unique: "2",
    entityType: "example",
    name: "Example 2",
    icon: "icon-shape-triangle",
    parent: {unique: null, entityType: "example-root"},
    isFolder: false,
    hasChildren: false,
  },
  {
    unique: "3",
    entityType: "example",
    name: "Example 3",
    icon: "icon-shape-triangle",
    parent: {unique: null, entityType: "example-root"},
    isFolder: false,
    hasChildren: false,
  },
  {
    unique: "4",
    entityType: "example",
    name: "Example 4",
    icon: "icon-shape-triangle",
    parent: {unique: "6", entityType: "example-folder"},
    isFolder: false,
    hasChildren: false,
  },
  {
    unique: "5",
    entityType: "example",
    name: "Example 5",
    icon: "icon-shape-triangle",
    parent: {unique: "6", entityType: "example-folder"},
    isFolder: false,
    hasChildren: false,
  },
  {
    unique: "6",
    entityType: "example-folder",
    name: "Example Folder 1",
    parent: {unique: null, entityType: "example-root"},
    isFolder: true,
    hasChildren: true,
  },
];
