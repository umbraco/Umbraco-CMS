import { Meta } from '@storybook/web-components';
import { html } from 'lit-html';
import { umbDataTypeData } from '../../../../../../../mocks/data/data-type.data.js';

import './property-editor-ui-tiny-mce-stylesheets-configuration.element.js';
import { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

const dataTypeData = umbDataTypeData.getById('dt-richTextEditor') as DataTypeResponseModel;

export default {
	title: 'Property Editor UIs/Tiny Mce Stylesheets Configuration',
	component: 'umb-property-editor-ui-tiny-mce-stylesheets-configuration',
	id: 'umb-property-editor-ui-tiny-mce-stylesheets-configuration',
} as Meta;

export const AAAOverview = ({ value }: any) =>
	html`<umb-property-editor-ui-tiny-mce-stylesheets-configuration .value=${value}></umb-property-editor-ui-tiny-mce-stylesheets-configuration>`;
AAAOverview.storyName = 'Overview';
AAAOverview.args = {
	value: dataTypeData?.values?.find(x => x.alias === 'stylesheets')?.value ?? []
}