import {MediaValueBuilder} from './mediaValueBuilder';
import {MediaVariantBuilder} from './mediaVariantBuilder';

export class MediaBuilder {
  mediaValueBuilder: MediaValueBuilder[];
  mediaVariantBuilders: MediaVariantBuilder[];
  id: string;
  parentId: string;
  mediaTypeId: string;

  constructor() {
    this.mediaValueBuilder = [];
    this.mediaVariantBuilders = [];
  }

  addValue() {
    const builder = new MediaValueBuilder(this);
    this.mediaValueBuilder.push(builder);
    return builder;
  }

  addVariant() {
    const builder = new MediaVariantBuilder(this);
    this.mediaVariantBuilders.push(builder);
    return builder;
  }

  withId(id: string) {
    this.id = id;
    return this;
  }

  withParentId(parentId: string) {
    this.parentId = parentId;
    return this;
  }

  withMediaTypeId(mediaTypeId: string) {
    this.mediaTypeId = mediaTypeId;
    return this;
  }

  build() {
    return {
      values: this.mediaValueBuilder.map(builder => builder.build()),
      variants: this.mediaVariantBuilders.map(builder => builder.build()),
      id: this.id || null,
      parent: this.parentId ? {id: this.parentId} : null,
      mediaType: {id: this.mediaTypeId}
    };
  }
}