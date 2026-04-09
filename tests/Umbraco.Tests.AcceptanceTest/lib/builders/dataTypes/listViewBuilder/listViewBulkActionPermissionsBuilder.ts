import {ListViewDataTypeBuilder} from '../listViewDataTypeBuilder';

export class ListViewBulkActionPermissionsBuilder {
  parentBuilder: ListViewDataTypeBuilder;
  allowBulkCopy: boolean;
  allowBulkDelete: boolean;
  allowBulkMove: boolean;
  allowBulkPublish: boolean;
  allowBulkUnPublish: boolean;

  constructor(parentBuilder: ListViewDataTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withAllowBulkCopy(allowBulkCopy: boolean) {
    this.allowBulkCopy = allowBulkCopy;
    return this;
  }

  withAllowBulkDelete(allowBulkDelete: boolean) {
    this.allowBulkDelete = allowBulkDelete;
    return this;
  }

  withAllowBulkMove(allowBulkMove: boolean) {
    this.allowBulkMove = allowBulkMove;
    return this;
  }

  withAllowBulkPublish(allowBulkPublish: boolean) {
    this.allowBulkPublish = allowBulkPublish;
    return this;
  }

  withAllowBulkUnPublish(allowBulkUnPublish: boolean) {
    this.allowBulkUnPublish = allowBulkUnPublish;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  getValues() {
    return {
      allowBulkCopy: this.allowBulkCopy || false,
      allowBulkDelete: this.allowBulkDelete || false,
      allowBulkMove: this.allowBulkMove || false,
      allowBulkPublish: this.allowBulkPublish || false,
      allowBulkUnPublish: this.allowBulkUnPublish || false
    };
  }
}