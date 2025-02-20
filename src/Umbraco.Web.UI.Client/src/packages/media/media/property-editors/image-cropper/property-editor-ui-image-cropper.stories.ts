import type { UmbPropertyEditorUIImageCropperElement } from './property-editor-ui-image-cropper.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-image-cropper.element.js';

export default {
	title: 'Property Editor UIs/Image Cropper',
	component: 'umb-property-editor-ui-image-cropper',
	id: 'umb-property-editor-ui-image-cropper',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIImageCropperElement> = () =>
	html`<umb-property-editor-ui-image-cropper></umb-property-editor-ui-image-cropper>`;
AAAOverview.storyName = 'Overview';
