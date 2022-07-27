import './installer';
import './server';
import './upgrader';
import './user';

import { api } from '@airtasker/spot';

/* eslint-disable */
@api({ name: 'umbraco-backoffice-api', version: '1.0.0' })
class Api {}
