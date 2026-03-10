import {DataTypeBuilder} from './dataTypeBuilder';

export class SliderDataTypeBuilder extends DataTypeBuilder {
  minVal: number;
  maxVal: number;
  enableRange: boolean;
  initVal1: number;
  initVal2: number;
  step: number;

  constructor() {
    super();
    this.editorAlias = 'Umbraco.Slider';
    this.editorUiAlias = 'Umb.PropertyEditorUi.Slider';
  }

  withMinValue(minValue: number) {
    this.minVal = minValue;
    return this;
  }

  withMaxValue(maxValue: number) {
    this.maxVal = maxValue;
    return this;
  }

  withEnableRange(enableRange: boolean) {
    this.enableRange = enableRange;
    return this;
  }

  withInitialValueOne(initialValueOne: number) {
    this.initVal1 = initialValueOne;
    return this;
  }

  withInitialValueTwo(initialValueTwo: number) {
    if (this.enableRange) {
      this.initVal2 = initialValueTwo;
    }
    return this;
  }

  withStep(step: number) {
    this.step = step;
    return this;
  }

  getValues() {
    let values: any = [];
    values.push({
      alias: 'minVal',
      value: this.minVal || 0
    });
    values.push({
      alias: 'maxVal',
      value: this.maxVal || 0
    });
    values.push({
      alias: 'enableRange',
      value: this.enableRange || false
    });
    values.push({
      alias: 'initVal1',
      value: this.initVal1 || 0
    });
    values.push({
      alias: 'initVal2',
      value: this.initVal2 || 0
    });
    values.push({
      alias: 'step',
      value: this.step || 0
    });
    return values;
  }
}