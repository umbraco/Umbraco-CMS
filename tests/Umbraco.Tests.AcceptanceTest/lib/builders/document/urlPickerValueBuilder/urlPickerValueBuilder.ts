import {DocumentValueBuilder} from '../documentValueBuilder';

export class URLPickerValueBuilder{
  parentBuilder: DocumentValueBuilder;
  icon: string;
  name: string;
  published: boolean;
  queryString: string;
  target: string;
  trashed: boolean;
  type: string;
  unique: string;
  url: string;

  constructor(parentBuilder: DocumentValueBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withIcon(icon: string) {
    this.icon = icon;
    return this;
  }

  withName(name: string) {
    this.name = name;
    return this;
  }

  withPublished(published: boolean) {
    this.published = published;
    return this;
  }

  withQueryString(queryString: string) {
    this.queryString = queryString;
    return this;
  }

  withTarget(target: string) {
    this.target = target;
    return this;
  }

  withTrashed(trashed: boolean) {
    this.trashed = trashed;
    return this;
  }

  withType(type: string) {
    this.type = type;
    return this;
  }

  withUnique(unique: string) {
    this.unique = unique;
    return this;
  }

  withUrl(url: string) {
    this.url = url;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  getValue() {
    let value: any = {};

    value.icon = this.icon;
    value.name = this.name || null;
    value.published = this.published !== undefined ? this.published : true;
    value.queryString = this.queryString || null;
    value.target = this.target || null;
    value.trashed = this.trashed || false;
    value.type = this.type;
    value.unique = this.unique || null;
    value.url = this.url;

    return value;
  }
}
