import {MemberTypeCompositionBuilder} from './memberTypeCompositionBuilder';
import {MemberTypeContainerBuilder} from './memberTypeContainerBuilder';
import {MemberTypePropertyBuilder} from './memberTypePropertyBuilder';
import {ensureIdExists} from '../../helpers/BuilderUtils';

export class MemberTypeBuilder {
  alias: string;
  name: string;
  description: string;
  icon: string;
  allowedAsRoot: boolean;
  variesByCulture: boolean;
  variesBySegment: boolean;
  collectionId: string;
  isElement: boolean;
  memberTypePropertyBuilder: MemberTypePropertyBuilder[];
  memberTypeContainerBuilder: MemberTypeContainerBuilder[];
  id: string;
  memberTypeCompositionBuilder: MemberTypeCompositionBuilder[];

  constructor() {
    this.memberTypePropertyBuilder = [];
    this.memberTypeContainerBuilder = [];
    this.memberTypeCompositionBuilder = [];
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
    const builder = new MemberTypePropertyBuilder(this);
    this.memberTypePropertyBuilder.push(builder);
    return builder;
  }

  addContainer() {
    const builder = new MemberTypeContainerBuilder(this);
    this.memberTypeContainerBuilder.push(builder);
    return builder;
  }

  addComposition() {
    const builder = new MemberTypeCompositionBuilder(this);
    this.memberTypeCompositionBuilder.push(builder);
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
      icon: this.icon || 'icon-user',
      allowedAsRoot: this.allowedAsRoot || false,
      variesByCulture: this.variesByCulture || false,
      variesBySegment: this.variesBySegment || false,
      collection: this.collectionId ? {id: this.collectionId} : null,
      isElement: this.isElement || false,
      properties: this.memberTypePropertyBuilder.map((builder) => {
        return builder.build();
      }) || [],
      containers: this.memberTypeContainerBuilder.map((builder) => {
        return builder.build();
      }) || [],
      id: this.id,
      compositions: this.memberTypeCompositionBuilder.map((builder) => {
        return builder.build();
      }) || []
    }
  }
}