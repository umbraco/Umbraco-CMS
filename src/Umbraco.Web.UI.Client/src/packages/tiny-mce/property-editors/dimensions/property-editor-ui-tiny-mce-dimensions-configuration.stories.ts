import { umbDataTypeMockDb } from '../../../../mocks/data/data-type/data-type.db.js';
import type { Meta } from '@storybook/web-components';
import './property-editor-ui-tiny-mce-dimensions-configuration.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';
import type { UmbDataTypeDetailModel } from '@umbraco-cms/backoffice/data-type';

const dataTypeData = umbDataTypeMockDb.read('dt-richTextEditor') as unknown as UmbDataTypeDetailModel;

export default {
	title: 'Property Editor UIs/Tiny Mce Dimensions Configuration',
	component: 'umb-property-eDitor-ui-tiny-mce-dimensions-configuration',
	id: 'umb-property-editor-ui-tiny-mce-dimensions-configuration',
} as Meta;

export const AAAOverview = ({ value }: any) =>
	html`<umb-property-editor-ui-tiny-mce-dimensions-configuration
		.value=${value}></umb-property-editor-ui-tiny-mce-dimensions-configuration>`;
AAAOverview.storyName = 'Overview';
AAAOverview.args = {
	value: dataTypeData?.values?.find((x) => x.alias === 'dimensions')?.value,
};
