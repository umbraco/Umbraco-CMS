export class WebhookBuilder {
  enabled: boolean;
  name: string;
  description: string;
  url: string;
  contentTypeKeys: string[];
  headers: Record<string, string>;
  events: string[];

  constructor() {
    this.contentTypeKeys = [];
    this.headers = {};
    this.events = [];
  }

  withEnabled(enabled: boolean) {
    this.enabled = enabled;
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

  withUrl(url: string) {
    this.url = url;
    return this;
  }

  withContentTypeKey(contentTypeKey: string) {
    this.contentTypeKeys.push(contentTypeKey);
    return this;
  }

  withHeader(headerName: string, headerValue: string) {
    this.headers[headerName] = headerValue;
    return this;
  }

  withEventAlias(eventAlias: string) {
    this.events.push(eventAlias);
    return this;
  }

  build() {
    return {
      enabled: this.enabled !== undefined ? this.enabled : true,
      name: this.name,
      description: this.description || '',
      url: this.url,
      contentTypeKeys: this.contentTypeKeys,
      headers: this.headers,
      events: this.events,
    };
  }
}
