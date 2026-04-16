import type { UmbMockDataSet } from '../../mock-data-set.types.js';

import { data as webhook } from './webhook.data.js';
import { data as webhookEvent } from './webhook-event.data.js';
import { data as webhookDelivery } from './webhook-delivery.data.js';
import { data as user } from './user.data.js';
import { data as userGroup } from './user-group.data.js';
import { data as language } from './language.data.js';

export { webhook, webhookEvent, webhookDelivery, user, userGroup, language };

// Type assertion to ensure this module satisfies UmbMockDataSet
const _typeCheck: UmbMockDataSet = {
	webhook,
	webhookEvent,
	webhookDelivery,
	user,
	userGroup,
	language,
};
void _typeCheck;
