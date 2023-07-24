import { Meta } from '@storybook/web-components';
import { umbDataTypeData } from '../../../../../../../mocks/data/data-type.data.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-tiny-mce-toolbar-configuration.element.js';
import { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

const dataTypeData = umbDataTypeData.getById('dt-richTextEditor') as DataTypeResponseModel;

export default {
	title: 'Property Editor UIs/Tiny Mce Toolbar Configuration',
	component: 'umb-property-editor-ui-tiny-mce-toolbar-configuration',
	id: 'umb-property-editor-ui-tiny-mce-toolbar-configuration',
} as Meta;

export const AAAOverview = ({ value }: any) =>
	html`<umb-property-editor-ui-tiny-mce-toolbar-configuration
		.value=${value}></umb-property-editor-ui-tiny-mce-toolbar-configuration>`;

AAAOverview.storyName = 'Overview';

AAAOverview.args = {
	value: dataTypeData?.values?.find((x) => x.alias === 'toolbar')?.value ?? [],
};
