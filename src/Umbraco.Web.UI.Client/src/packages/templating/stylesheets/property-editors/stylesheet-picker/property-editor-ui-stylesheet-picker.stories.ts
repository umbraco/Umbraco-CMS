import { umbDataTypeMockDb } from '../../../../../mocks/data/data-type/data-type.db.js';
import { html } from '@umbraco-cms/backoffice/external/lit';
import type { Meta } from '@storybook/web-components';
import type { UmbDataTypeDetailModel } from '@umbraco-cms/backoffice/data-type';

import './property-editor-ui-stylesheet-picker.element.js';

const dataTypeData = umbDataTypeMockDb.read('dt-richTextEditor') as unknown as UmbDataTypeDetailModel;

export default {
	title: 'Property Editor UIs/Stylesheet Picker',
	component: 'umb-property-editor-ui-stylesheet-picker',
	id: 'umb-property-editor-ui-stylesheet-picker',
} as Meta;

export const AAAOverview = ({ value }: any) =>
	html`<umb-property-editor-ui-stylesheet-picker .value=${value}></umb-property-editor-ui-stylesheet-picker>`;
AAAOverview.storyName = 'Overview';
AAAOverview.args = {
	value: dataTypeData?.values?.find((x) => x.alias === 'stylesheets')?.value ?? [],
};
