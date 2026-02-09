import {UmbControllerBase} from "@umbraco-cms/backoffice/class-api";

export class ExampleCustomPickerCollectionPropertyEditorDataSource extends UmbControllerBase {
  collectionPickableFilter = (item) => item.isPickable;

  async requestCollection(args) {
    const data = {
      items: customItems,
      total: customItems.length,
    };

    return {data};
  }

  async requestItems(uniques) {
    const items = customItems.filter((x) => uniques.includes(x.unique));
    return {data: items};
  }

  async search(args) {
    const items = customItems.filter((item) =>
      item.name?.toLowerCase().includes(args.query.toLowerCase())
    );
    const total = items.length;

    const data = {
      items,
      total,
    };

    return {data};
  }
}

export {ExampleCustomPickerCollectionPropertyEditorDataSource as api};

const customItems = [
  {
    unique: "1",
    entityType: "example",
    name: "Example 1",
    icon: "icon-shape-triangle",
    isPickable: true,
  },
  {
    unique: "2",
    entityType: "example",
    name: "Example 2",
    icon: "icon-shape-triangle",
    isPickable: true,
  },
  {
    unique: "3",
    entityType: "example",
    name: "Example 3",
    icon: "icon-shape-triangle",
    isPickable: true,
  },
  {
    unique: "4",
    entityType: "example",
    name: "Example 4",
    icon: "icon-shape-triangle",
    isPickable: false,
  },
  {
    unique: "5",
    entityType: "example",
    name: "Example 5",
    icon: "icon-shape-triangle",
    isPickable: true,
  },
];
