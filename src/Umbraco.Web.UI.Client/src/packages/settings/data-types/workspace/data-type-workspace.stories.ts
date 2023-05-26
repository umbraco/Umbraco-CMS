import './data-type-workspace.element.js';

import { Meta, Story } from '@storybook/web-components';
import { data } from '../../../../mocks/data/data-type.data.js';
import type { UmbDataTypeWorkspaceElement } from './data-type-workspace.element.js';
import { html , ifDefined } from '@umbraco-cms/backoffice/external/lit';



export default {
	title: 'Workspaces/Data Type',
	component: 'umb-data-type-workspace',
	id: 'umb-data-type-workspace',
} as Meta;

export const AAAOverview: Story<UmbDataTypeWorkspaceElement> = () =>
	html` <umb-data-type-workspace id="${ifDefined(data[0].id)}"></umb-data-type-workspace>`;
AAAOverview.storyName = 'Overview';
