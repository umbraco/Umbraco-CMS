import {MediaTypeBuilder} from './mediaTypeBuilder';
import {buildContainer} from '../../helpers/BuilderUtils';

export class MediaTypeContainerBuilder {
  parentBuilder: MediaTypeBuilder;
  id: string;
  parentId: string;
  name: string;
  type: string;
  sortOrder: number;

  constructor(parentBuilder: MediaTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withId(id: string) {
    this.id = id;
    return this;
  }

  withParentId(parentId: string) {
    this.parentId = parentId;
    return this;
  }

  withName(name: string) {
    this.name = name;
    return this;
  }

  withType(type: string) {
    this.type = type;
    return this;
  }

  withSortOrder(sortOrder: number) {
    this.sortOrder = sortOrder;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    return buildContainer({
      id: this.id,
      parentId: this.parentId,
      name: this.name,
      type: this.type,
      sortOrder: this.sortOrder
    });
  }
}