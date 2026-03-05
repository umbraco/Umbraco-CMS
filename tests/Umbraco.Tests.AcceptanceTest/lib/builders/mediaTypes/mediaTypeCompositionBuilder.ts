import {MediaTypeBuilder} from './mediaTypeBuilder';
import {buildComposition} from '../../helpers/BuilderUtils';

export class MediaTypeCompositionBuilder {
  parentBuilder: MediaTypeBuilder;
  mediaTypeId: string
  compositionType: string;

  constructor(parentBuilder: MediaTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withMediaTypeId(mediaTypeId: string) {
    this.mediaTypeId = mediaTypeId;
    return this;
  }

  withCompositionType(compositionType: string) {
    this.compositionType = compositionType;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    return buildComposition('mediaType', this.mediaTypeId, this.compositionType);
  }
}