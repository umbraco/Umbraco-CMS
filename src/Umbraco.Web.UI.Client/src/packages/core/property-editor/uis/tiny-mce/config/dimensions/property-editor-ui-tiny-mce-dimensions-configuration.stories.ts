import { Meta } from '@storybook/web-components';
import './property-editor-ui-tiny-mce-dimensions-configuration.element.js';
import { umbDataTypeData } from '../../../../../../../mocks/data/data-type.data.js';
import { DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { html } from '@umbraco-cms/backoffice/external/lit';

const dataTypeData = umbDataTypeData.getById('dt-richTextEditor') as DataTypeResponseModel;

export default {
	title: 'Property Editor UIs/Tiny Mce Dimensions Configuration',
	component: 'umb-property-eDitor-ui-tiny-mce-dimensions-configuration',
	id: 'umb-property-editor-ui-tiny-mce-dimensions-configuration',
} as Meta;

export const AAAOverview = ({ value }: any) =>
	html`<umb-property-editor-ui-tiny-mce-dimensions-configuration .value=${value}></umb-property-editor-ui-tiny-mce-dimensions-configuration>`;
AAAOverview.storyName = 'Overview';
AAAOverview.args = {
	value: dataTypeData?.values?.find((x) => x.alias === 'dimensions')?.value,
};
