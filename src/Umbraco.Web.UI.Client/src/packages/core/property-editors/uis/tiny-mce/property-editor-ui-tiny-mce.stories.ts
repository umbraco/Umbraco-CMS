import { Meta } from '@storybook/web-components';
import type { UmbPropertyEditorUITinyMceElement } from './property-editor-ui-tiny-mce.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-tiny-mce.element';
import { umbDataTypeData } from '../../../../../shared/mocks/data/data-type.data';
import { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

const dataTypeData = umbDataTypeData.getById('dt-richTextEditor') as DataTypeResponseModel;

export default {
	title: 'Property Editor UIs/Tiny Mce',
	component: 'umb-property-editor-ui-tiny-mce',
	id: 'umb-property-editor-ui-tiny-mce',
} as Meta;

export const AAAOverview = ({ config, value }: any) =>
	html`<umb-property-editor-ui-tiny-mce .config=${config} .value=${value}></umb-property-editor-ui-tiny-mce>`;
	
AAAOverview.storyName = 'Overview';

AAAOverview.args = {
	config: dataTypeData?.values,
	value: 'I am a default value for the TinyMCE text editor story.',
}