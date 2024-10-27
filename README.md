该项目实现了一个简易的AI对话功能，主要的功能上包括向服务器发送指定最大长度的历史对话，服务端回复指定最大长度的单个对话。
前端会及时显示流式返回的结果，并且具有打字机和对话气泡效果。另外根据个人喜好添加了AI的形象，动作，人设和BGM（做了一个小播放器）。
历史对话会自动进行裁切和保存到本地，但是历史对话和发送给AI的对话是同一份数据，这意味着历史对话的保存长度也会受限，有需求可以自己改一下。
AI的执行请求流程部分的代码是根据网络上的文章结合星火大模型的官方教程和C#示例代码完成的。

This project has implemented a simple AI conversation function, which mainly includes sending historical conversations of a specified maximum length to the server, and the server replying to individual conversations of a specified maximum length.
The front-end will display the results returned by streaming in a timely manner, and has typewriter and dialogue bubble effects. In addition, AI images, actions, character designs, and BGM were added based on personal preferences (making a small player).
The historical dialogue will be automatically cropped and saved locally, but the historical dialogue and the dialogue sent to AI are the same data, which means that the storage length of the historical dialogue will also be limited. If there is a need, you can change it yourself.
The code for the execution request process of AI is completed based on articles on the internet, combined with the official tutorial of Spark Model and C # example code.
