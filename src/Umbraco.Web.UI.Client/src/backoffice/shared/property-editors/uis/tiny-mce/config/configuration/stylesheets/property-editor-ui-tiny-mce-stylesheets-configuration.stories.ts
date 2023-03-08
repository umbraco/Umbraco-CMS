import { Meta } from '@storybook/web-components';
import { html } from 'lit-html';
import { umbDataTypeData } from 'src/core/mocks/data/data-type.data';

import './property-editor-ui-tiny-mce-stylesheets-configuration.element';

const dataTypeData = umbDataTypeData.getByKey('dt-richTextEditor');

export default {
	title: 'Property Editor UIs/Tiny Mce Stylesheets Configuration',
	component: 'umb-property-editor-ui-tiny-mce-stylesheets-configuration',
	id: 'umb-property-editor-ui-tiny-mce-stylesheets-configuration',
} as Meta;

export const AAAOverview = ({ value }: any) =>
	html`<umb-property-editor-ui-tiny-mce-stylesheets-configuration .value=${value}></umb-property-editor-ui-tiny-mce-stylesheets-configuration>`;
AAAOverview.storyName = 'Overview';
AAAOverview.args = {
	value: dataTypeData?.data?.find(x => x.alias === 'stylesheets')?.value ?? []
}