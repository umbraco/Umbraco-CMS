import { umbDataTypeMockDb } from '../../../../mocks/data/data-type/data-type.db.js';
import type { Meta } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-tiny-mce-toolbar-configuration.element.js';
import type { UmbDataTypeDetailModel } from '@umbraco-cms/backoffice/data-type';

const dataTypeData = umbDataTypeMockDb.read('dt-richTextEditor') as unknown as UmbDataTypeDetailModel;

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
