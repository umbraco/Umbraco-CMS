import { Meta } from '@storybook/web-components';
import { html } from 'lit-html';
import { umbDataTypeData } from '../../../../../../../mocks/data/data-type.data.js';

import './property-editor-ui-tiny-mce-maximagesize-configuration.element.js';
import { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

const dataTypeData = umbDataTypeData.getById('dt-richTextEditor') as DataTypeResponseModel;

export default {
	title: 'Property Editor UIs/Tiny Mce Max Image Size Configuration',
	component: 'umb-property-eDitor-ui-tiny-mce-maximagesize-configuration',
	id: 'umb-property-editor-ui-tiny-mce-maximagesize-configuration',
} as Meta;

export const AAAOverview = ({ value }: any) =>
	html`<umb-property-editor-ui-tiny-mce-maximagesize-configuration
		.value=${value}></umb-property-editor-ui-tiny-mce-maximagesize-configuration>`;
AAAOverview.storyName = 'Overview';
AAAOverview.args = {
	value: dataTypeData?.values?.find((x) => x.alias === 'maxImageSize')?.value,
};
