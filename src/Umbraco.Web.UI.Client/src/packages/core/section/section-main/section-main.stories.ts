import type { UmbSectionMainElement } from './section-main.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './section-main.element.js';

export default {
	title: 'Sections/Shared/Section Main',
	component: 'umb-section-main',
	id: 'umb-section-main',
} as Meta;

export const AAAOverview: StoryFn<UmbSectionMainElement> = () => html`
	<umb-section-main>Section Main Area</umb-section-main>
`;
AAAOverview.storyName = 'Overview';
