import {MediaTypePropertyBuilder} from './mediaTypePropertyBuilder';
import {MediaTypeContainerBuilder} from './mediaTypeContainerBuilder';
import {MediaTypeAllowedMediaTypeBuilder} from './mediaTypeAllowedMediaTypeBuilder';
import {MediaTypeCompositionBuilder} from './mediaTypeCompositionBuilder';
import {ensureIdExists} from '../../helpers/BuilderUtils';

export class MediaTypeBuilder {
  alias: string;
  name: string;
  description: string;
  icon: string;
  allowedAsRoot: boolean;
  variesByCulture: boolean;
  variesBySegment: boolean;
  collectionId: string;
  isElement: boolean;
  mediaTypePropertyBuilder: MediaTypePropertyBuilder[];
  mediaTypeContainerBuilder: MediaTypeContainerBuilder[];
  mediaTypeAllowedMediaTypeBuilder: MediaTypeAllowedMediaTypeBuilder[];
  mediaTypeCompositionBuilder: MediaTypeCompositionBuilder[];
  folderId: string;
  id: string;

  constructor() {
    this.mediaTypePropertyBuilder = [];
    this.mediaTypeContainerBuilder = [];
    this.mediaTypeAllowedMediaTypeBuilder = [];
    this.mediaTypeCompositionBuilder = [];
  }

  withAlias(alias: string) {
    this.alias = alias;
    return this;
  }

  withName(name: string) {
    this.name = name;
    return this;
  }

  withDescription(description: string) {
    this.description = description;
    return this;
  }

  withIcon(icon: string) {
    this.icon = icon;
    return this;
  }

  withAllowedAsRoot(allowedAsRoot: boolean) {
    this.allowedAsRoot = allowedAsRoot;
    return this;
  }

  withVariesByCulture(variesByCulture: boolean) {
    this.variesByCulture = variesByCulture;
    return this;
  }

  withVariesBySegment(variesBySegment: boolean) {
    this.variesBySegment = variesBySegment;
    return this;
  }

  withIsElement(isElement: boolean) {
    this.isElement = isElement;
    return this;
  }

  addProperty() {
    const builder = new MediaTypePropertyBuilder(this);
    this.mediaTypePropertyBuilder.push(builder);
    return builder;
  }

  addContainer() {
    const builder = new MediaTypeContainerBuilder(this);
    this.mediaTypeContainerBuilder.push(builder);
    return builder;
  }

  addAllowedMediaType() {
    const builder = new MediaTypeAllowedMediaTypeBuilder(this);
    this.mediaTypeAllowedMediaTypeBuilder.push(builder);
    return builder;
  }

  addComposition() {
    const builder = new MediaTypeCompositionBuilder(this);
    this.mediaTypeCompositionBuilder.push(builder);
    return builder;
  }

  withId(id: string) {
    this.id = id;
    return this;
  }

  build() {
    this.id = ensureIdExists(this.id);

    return {
      alias: this.alias || '',
      name: this.name || '',
      description: this.description || '',
      icon: this.icon || 'icon-document',
      allowedAsRoot: this.allowedAsRoot || false,
      variesByCulture: this.variesByCulture || false,
      variesBySegment: this.variesBySegment || false,
      collection: this.collectionId ? {id: this.collectionId} : null,
      isElement: this.isElement || false,
      properties: this.mediaTypePropertyBuilder.map((builder) => {
        return builder.build();
      }) || [],
      containers: this.mediaTypeContainerBuilder.map((builder) => {
        return builder.build();
      }) || [],
      allowedMediaTypes: this.mediaTypeAllowedMediaTypeBuilder.map((builder) => {
        return builder.build();
      }) || [],
      compositions: this.mediaTypeCompositionBuilder.map((builder) => {
        return builder.build();
      }) || [],
      id: this.id,
      folder: this.folderId ? {id: this.folderId} : null,
    }
  }
}