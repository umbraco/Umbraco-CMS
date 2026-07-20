import {DocumentTypeBuilder} from './documentTypeBuilder';
import {ensureIdExists, buildProperty} from '../../helpers/BuilderUtils';

export class DocumentTypePropertyBuilder {
  parentBuilder: DocumentTypeBuilder;
  id: string;
  containerId: string;
  sortOrder: number;
  alias: string;
  name: string;
  description: string;
  dataTypeId: string;
  variesByCulture: boolean;
  variesBySegment: boolean;
  mandatory: boolean;
  mandatoryMessage: string;
  regEx: string;
  regExMessage: string;
  labelOnTop: boolean;

  constructor(parentBuilder: DocumentTypeBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withId(id: string) {
    this.id = id;
    return this;
  }

  withContainerId(containerId: string) {
    this.containerId = containerId;
    return this;
  }

  withSortOrder(sortOrder: number) {
    this.sortOrder = sortOrder;
    return this;
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

  withDataTypeId(dataTypeId: string) {
    this.dataTypeId = dataTypeId;
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

  withMandatory(mandatory: boolean) {
    this.mandatory = mandatory;
    return this;
  }

  withMandatoryMessage(mandatoryMessage: string) {
    this.mandatoryMessage = mandatoryMessage;
    return this;
  }

  withRegEx(regEx: string) {
    this.regEx = regEx;
    return this;
  }

  withRegExMessage(regExMessage: string) {
    this.regExMessage = regExMessage;
    return this;
  }

  withLabelOnTop(labelOnTop: boolean) {
    this.labelOnTop = labelOnTop;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    this.id = ensureIdExists(this.id);

    return buildProperty({
      id: this.id,
      containerId: this.containerId,
      sortOrder: this.sortOrder,
      alias: this.alias,
      name: this.name,
      description: this.description,
      dataTypeId: this.dataTypeId,
      variesByCulture: this.variesByCulture,
      variesBySegment: this.variesBySegment,
      mandatory: this.mandatory,
      mandatoryMessage: this.mandatoryMessage,
      regEx: this.regEx,
      regExMessage: this.regExMessage,
      labelOnTop: this.labelOnTop
    });
  }
}
