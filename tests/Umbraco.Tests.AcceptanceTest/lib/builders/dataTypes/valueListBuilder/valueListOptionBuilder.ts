export class ValueListOptionBuilder<TParent> {
  parentBuilder: TParent;
  value: string;
  label: string;

  constructor(parentBuilder: TParent) {
    this.parentBuilder = parentBuilder;
  }

  withValue(value: string) {
    this.value = value;
    return this;
  }

  withLabel(label: string) {
    this.label = label;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  getValues() {
    return {value: this.value, label: this.label};
  }
}
