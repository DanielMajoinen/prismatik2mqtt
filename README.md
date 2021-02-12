# prismatik2mqtt

1. Enable server (API via sockets) in Prismatik -> Settings -> Experimental. 

2. Modify appsettings.json or use environment variables to set configuration to your needs.

3. Add the following light to home assistant:
```
light:
  - platform: mqtt
    state_topic: "prismatik2mqtt/daniel-monitor/state"
    command_topic: "prismatik2mqtt/daniel-monitor/cmd"
    name: "Daniel Monitor Backlight"
    qos: 0
    payload_on: "ON"
    payload_off: "OFF"
```
