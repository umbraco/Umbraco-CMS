import {DataTypeBuilder} from './dataTypeBuilder';
import {MediaPickerCropBuilder} from './mediaPickerBuilder';

// There is one media picker editor per number of items it holds, so the two builders differ in which
// editor they target, and only the multiple picker has an item count to validate.
export class MediaPickerDataTypeBuilder extends DataTypeBuilder {
  filter: string;
  minValue: number;
  maxValue: number;
  enableLocalFocalPoint: boolean;
  ignoreUserStartNodes: boolean;
  startNodeId: string;
  mediaPickerCropBuilder: MediaPickerCropBuilder[];

  constructor() {
    super();
    this.editorAlias = 'Umbraco.MediaPicker3';
    this.editorUiAlias = 'Umb.PropertyEditorUi.MediaPicker';
    this.mediaPickerCropBuilder = [];
  }

  withFilter(filter: string) {
    this.filter = filter;
    return this;
  }

  withMinValue(minValue: number) {
    this.minValue = minValue;
    return this;
  }

  withMaxValue(maxValue: number) {
    this.maxValue = maxValue;
    return this;
  }

  withEnableLocalFocalPoint(enableLocalFocalPoint: boolean) {
    this.enableLocalFocalPoint = enableLocalFocalPoint;
    return this;
  }

  withIgnoreUserStartNodes(ignoreUserStartNodes: boolean) {
    this.ignoreUserStartNodes = ignoreUserStartNodes;
    return this;
  }

  withStartNodeId(startNodeId: string) {
    this.startNodeId = startNodeId;
    return this;
  }

  addCrop() {
    const builder = new MediaPickerCropBuilder(this);
    this.mediaPickerCropBuilder.push(builder);
    return builder;
  }

  getValues() {
    let values: any[] = [];
    if (this.filter !== undefined) {
      values.push({
        alias: 'filter',
        value: this.filter
      });
    }
    if (this.minValue !== undefined || this.maxValue !== undefined) {
      values.push({
        alias: 'validationLimit',
        value: {
          min: this.minValue !== undefined ? this.minValue : '',
          max: this.maxValue !== undefined ? this.maxValue : ''
        }
      });
    }
    if (this.enableLocalFocalPoint !== undefined) {
      values.push({
        alias: 'enableLocalFocalPoint',
        value: this.enableLocalFocalPoint !== undefined ? this.enableLocalFocalPoint : false
      });
    }
    if (this.ignoreUserStartNodes !== undefined) {
      values.push({
        alias: 'ignoreUserStartNodes',
        value: this.ignoreUserStartNodes !== undefined ? this.ignoreUserStartNodes : false
      });
    }
    if (this.startNodeId !== undefined) {
      values.push({
        alias: 'startNodeId',
        value: this.startNodeId
      });
    }
    if (this.mediaPickerCropBuilder && this.mediaPickerCropBuilder.length > 0) {
      values.push({
        alias: 'crops',
        value: this.mediaPickerCropBuilder.map((builder) => {
          return builder.getValues();
        })
      });
    }
    return values;
  }
}

export class SingleMediaPickerDataTypeBuilder extends MediaPickerDataTypeBuilder {
  constructor() {
    super();
    this.editorAlias = 'Umbraco.MediaPicker.Single';
    this.editorUiAlias = 'Umb.PropertyEditorUi.MediaPicker.Single';
  }
}
