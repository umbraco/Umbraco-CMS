import {ListViewDataTypeBuilder} from '../listViewDataTypeBuilder';

export class ListViewLayoutBuilder {
  parentBuilder: ListViewDataTypeBuilder;
  collectionView: string;
  icon: string;
  name: string;

  constructor(parentBuilder: ListViewDataTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withCollectionView(collectionView: string) {
    this.collectionView = collectionView;
    return this;
  }

  withName(name: string) {
    this.name = name;
    return this;
  }

  withIcon(icon: string) {
    this.icon = icon;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  getValues() {
    return {
      collectionView: this.collectionView || 'Umb.CollectionView.Document.Table',
      icon: this.icon || 'icon-list',
      name: this.name || null || 'List'
    };
  }
}